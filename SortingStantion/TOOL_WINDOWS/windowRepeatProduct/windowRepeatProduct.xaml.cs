using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Models;
using System.Windows;

namespace SortingStantion.TOOL_WINDOWS.windowRepeatProduct
{
    /// <summary>
    /// Логика взаимодействия для windowRepeatProduct.xaml
    /// </summary>
    public partial class windowRepeatProduct : Window
    {

        /// <summary>
        /// Указательн на сообщение
        /// в зоне информации, которое надо удалить
        /// </summary>
        UserMessage userMessage;

        /// <summary>
        /// Тэг, хранящий количество изделий, отбраковыных вручную
        /// </summary>
        S7_DWord QUANTITY_PRODUCTS_MANUAL_REJECTED;

        /// <summary>
        /// GTIN
        /// </summary>
        string GTIN;

        /// <summary>
        /// SerialNumber
        /// </summary>
        string serialnumber;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="GTIN"></param>
        /// <param name="serialnumber"></param>
        /// <param name="userMessage"></param>
        public windowRepeatProduct(string GTIN, string serialnumber, UserMessage userMessage)
        {
            //Инициализация UI
            InitializeComponent();

            //Перенос полей в буфер
            this.serialnumber = serialnumber;
            this.GTIN = GTIN;

            //Передача указателя на сообщение в зоне
            //информации, которое надо удалить при нажатии кнопки Отмена
            this.userMessage = userMessage;

            //Подписка на событие по приему данных от ручного сканера
            DataBridge.Scaner.NewDataNotification += Scaner_NewDataNotification;

            //Передача указателя на окно, в центе которого 
            //надо разместить окна
            this.Owner = DataBridge.MainScreen;

            //Формирование правиьного сообщения
            txMessage.Text = $"     Продукт GTIN {GTIN} номер {serialnumber} считан повторно. Найдите его ручным сканером и удалите с конвейера.";

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
                Если код уже содержится в результате
            */
            if (DataBridge.Report.AsAResult(serialnumber) == true)
            {
                //Вывод сообщения в окно информации
                string message = $"Продукт GTIN {GTIN} номер {serialnumber} в результате.";
                var msg = new UserMessage(message, DataBridge.myRed);
                DataBridge.MSGBOX.Add(msg);

                //Выход из функции
                return;
            }

            /*
                Если код считан повторно
            */
            if (this.serialnumber == serialnumber)
            {
                //Вывод сообщения в окно информации
                string message = $"Продукт номер {serialnumber} считан повторно. Удалите его с конвейера.";
                var msg = new UserMessage(message, DataBridge.myGreen);
                DataBridge.MSGBOX.Add(msg);

                //Выход из функции
                return;
            }


            /*
                Если код доступен для сериализации
            */
            if (DataBridge.Report.AsAResult(serialnumber) == true)
            {
                //Вывод сообщения в окно информации
                string message = $"Продукт GTIN {gtin} номер {serialnumber} доступен для сериализации.";
                var msg = new UserMessage(message, DataBridge.myRed);
                DataBridge.MSGBOX.Add(msg);

                //Выход из функции
                return;
            }
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
