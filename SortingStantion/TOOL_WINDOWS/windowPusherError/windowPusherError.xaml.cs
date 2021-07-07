using SortingStantion.Controls;
using SortingStantion.Models;
using SortingStantion.S7Extension;
using System;
using System.Windows;
using System.Windows.Media;

namespace SortingStantion.TOOL_WINDOWS.windowPusherError
{
    /// <summary>
    /// Логика взаимодействия для windowPusherError.xaml
    /// </summary>
    public partial class windowPusherError : Window
    {

        /// <summary>
        /// Ошибка, которую надо квитировать
        /// </summary>
        S7DiscreteAlarm alarm;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public windowPusherError(S7DiscreteAlarm alarm)
        {
            //Инициализация UI
            InitializeComponent();

            //Передача окна, в уентре которого выводить окно
            this.Owner = DataBridge.MainScreen;

            //Подписка на обытие по получению данных от сканера
            DataBridge.Scaner.NewDataNotification += Scaner_NewDataNotification;

            //Передача указателя на ошибку,
            //которую требуется подтвердить
            this.alarm = alarm;

            //Выражение, вызываемое при закрытии
            //данного экземпляра окна
            this.Closing += (e, s) =>
            {
                //Отписка от обытия по получению данных от сканера
                DataBridge.Scaner.NewDataNotification -= Scaner_NewDataNotification;
            };
        }

        /// <summary>
        /// Метод, вызываемый при получении данных
        /// от сканера
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
                var msg = new UserMessage(message, DataBridge.myGreen);
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
                Если код в результате. 
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
        /// Метод, вызываемый по клику на кнопку
        /// ДОБАВИТЬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Квитирование ошибки
            alarm.Write(false);

            //Закрытие окна
            this.Close();
        }

        /// <summary>
        /// Метод для отображения сообщения в зоне информации
        /// </summary>
        /// <param name="message"></param>
        void ShowMessageInformationZone(string message, Brush brush)
        {
            Action action = () =>
            {
                var msgitem = new UserMessage(message, brush);
                DataBridge.MSGBOX.Add(msgitem);
            };
            DataBridge.UIDispatcher.Invoke(action);
        }
    }
}
