using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Models;
using System.Windows;

namespace SortingStantion.TOOL_WINDOWS.windowGtinFault
{
    /// <summary>
    /// Логика взаимодействия для frame_gtin_fault.xaml
    /// </summary>
    public partial class windowGtinFault : Window
    {
        /// <summary>
        /// Указательн на сообщение
        /// в зоне информации, которое надо удалить
        /// </summary>
        UserMessage userMessage;

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticServer server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Тэг, хранящий количество изделий, отбраковыных вручную
        /// </summary>
        S7_DWord QUANTITY_PRODUCTS_MANUAL_REJECTED;

        /// <summary>
        /// Конструктор
        /// </summary>
        public windowGtinFault(string GTIN, string serialNumber, UserMessage userMessage)
        {
            //Инициализация UI
            InitializeComponent();

            //Количество изделий, отбракованых вручную
            QUANTITY_PRODUCTS_MANUAL_REJECTED = (S7_DWord)device.GetTagByAddress("DB1.DBD28");

            //Передача указателя на сообщение в зоне
            //информации, которое надо удалить при нажатии кнопки Отмена
            this.userMessage = userMessage;

            //Подписка на событие по приему данных от ручного сканера
            DataBridge.Scaner.NewDataNotification += Scaner_NewDataNotification;

            //Передача указателя на окно, в центе которого 
            //надо разместить окна
            this.Owner = DataBridge.MainScreen;

            //Формирование правиьного сообщения
            txMessage.Text = $"     Считан посторонний продукт GTIN {GTIN} номер {serialNumber}. Найдите его ручным сканером и удалите с конвейера.";

            //Подписка на события
            this.Closing += Window_Closing;
        }

        /// <summary>
        /// Метод, вызываемый при получении новых
        /// данных от ручного сканера
        /// </summary>
        /// <param name="obj"></param>
        private void Scaner_NewDataNotification(string inputdata)
        {
            //Инициализируем разделитель по полям
            var spliter = new DataSpliter();

            //Копируем входные данные в буфер
            var istr = inputdata;

            //Разделяем входные данные по полям
            spliter.Split(ref istr);

            /*
               Если код не распознан
           */
            if (spliter.IsValid == false)
            {
                //Вывод сообщения в окно информации
                string message = $"Код не распознан. Удалите продукт с конвейера.";
                var msg = new UserMessage(message, DataBridge.myRed);
                DataBridge.MSGBOX.Add(msg);

                //Выход из функции
                return;
            }

            var gtin = spliter.GTIN;
            var serialnumber = spliter.SerialNumber;

            /*
                Если Посторонний продукт
            */
            if (DataBridge.WorkAssignmentEngine.GTIN != gtin)
            {
                //Вывод сообщения в окно информации
                string message = $"Посторонний продукт GTIN {gtin} номер {serialnumber}. Удалите его с конвейера.";
                var msg = new UserMessage(message, DataBridge.myGreen);
                DataBridge.MSGBOX.Add(msg);

                //Выход из функции
                return;
            }

            /*
                Если код не распознан. 
            */
            if (DataBridge.Report.AsAResult(serialnumber) == true)
            {
                //Вывод сообщения в окно информации
                string message = $"Продукт GTIN {gtin} номер {serialnumber} в результате.";
                var msg = new UserMessage(message, DataBridge.myRed);
                DataBridge.MSGBOX.Add(msg);

                //Выход из функции
                return;
            }

            /*
                Если код доступен для сериализации
            */
            //if (DataBridge.Report.AsAResult(serialnumber) == true)
            //{
            //    //Вывод сообщения в окно информации
            //    string message = $"Продукт GTIN {gtin} номер {serialnumber} доступен для сериализации.";
            //    var msg = new UserMessage(message, DataBridge.myRed);
            //    DataBridge.MSGBOX.Add(msg);

            //    //Выход из функции
            //    return;
            //}
        }


        /// <summary>
        /// Метод, вызываемый при клике по кнопке - ОТМЕНА
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DataBridge.MSGBOX.Remove(userMessage);
            this.Closing -= Window_Closing;
            DataBridge.Scaner.NewDataNotification -= Scaner_NewDataNotification;

            //Инкремент счетчика отбракованых изделий вручную
            var value = (int)QUANTITY_PRODUCTS_MANUAL_REJECTED.Status + 1;
            QUANTITY_PRODUCTS_MANUAL_REJECTED.Write(value);

            this.Close();
        }

        /// <summary>
        /// Метод, вызываемый при закрытии окна
        /// (отмена закрытия окна)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
