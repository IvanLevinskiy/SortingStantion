using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SortingStantion.windiwAddingBox
{
    /// <summary>
    /// Логика взаимодействия для windiwAddingBox.xaml
    /// </summary>
    public partial class windiwAddingBox : Window
    {
        DispatcherTimer ShutdownTimer;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public windiwAddingBox()
        {
            //Инициализация UI
            InitializeComponent();

            //Подписка на обытие по получению данных от сканера
            DataBridge.Scaner.NewDataNotification += Scaner_NewDataNotification;

            //Инициализация и запуск таймера для закрытия окна
            //при бездейсвии
            ShutdownTimer = new DispatcherTimer();
            int WindowTimeOut = GetWindowTimeOut();
            ShutdownTimer.Interval = new TimeSpan(0, 0, WindowTimeOut);
            ShutdownTimer.Tick += ShutdownTimer_Tick;
            ShutdownTimer.Start();

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
        private void Scaner_NewDataNotification(string datastring)
        {
            //Сброс таймера отсчитывающего временя бездействия
            TimerReset();

            //Текст сообщения в зоне информации
            string message = string.Empty;

            //Инициализируем разделитель по полям
            var spliter = new DataSpliter();

            //Копируем входные данные в буфер
            var istr = datastring;

            //Разделяем входные данные по полям
            spliter.Split(ref istr);

            /*
                Посторонний код
            */
            if (spliter.IsValid == false)
            {
                //Вывод сообщения в окно информации
                message = $"Код не распознан.";
                ShowMessage(message, DataBridge.myBlue);

                //Выход из функции
                return;
            }

            //Получение GTIN и SN
            var gtin = spliter.GTIN;
            var serialnumber = spliter.SerialNumber;

            /*
                Если Посторонний продукт
            */
            if (DataBridge.WorkAssignmentEngine.GTIN != gtin)
            {
                //Вывод сообщения в окно информации
                message = $"Посторонний продукт не может быть добавлен в результат.";
                ShowMessage(message, DataBridge.myBlue);

                //Выход из функции
                return;
            }

            /*
                Продукт в браке
            */
            if (DataBridge.Report.IsDeffect(serialnumber) == true)
            {
                message = $"Продукт номер {serialnumber} числиться в браке.";
                ShowMessage(message, DataBridge.myBlue);

                //Выход
                return;
            }

            /*
                Продукт уже содержится в результате
            */
            if (DataBridge.Report.AsAResult(serialnumber) == true)
            {
                //Формирование сообщения
                message = $"Продукт {serialnumber} уже есть в результате.";
                ShowMessage(message, DataBridge.myBlue);

                //Выход
                return;
            }

            /*
                Добавление кода в результ
            */
            DataBridge.Report.AddBox(serialnumber);

            /*
                Текст сообщения
            */
            message = $"Продукт {serialnumber} добавлен в результат.";

            /*
                Добавление в базу данных (лог) записи
            */
            DataBridge.AlarmLogging.AddMessage(message, Models.MessageType.Info);

            /*
                Добавление сообщения в зону информации
            */
            ShowMessage(message, DataBridge.myOrange);

        }

        /// <summary>
        /// Метод для отображения сообщения в зоне информации
        /// </summary>
        /// <param name="message"></param>
        void ShowMessage(string message, Brush color)
        {
            Action action = () =>
            {
                var msgitem = new UserMessage(message, color);
                DataBridge.MSGBOX.Add(msgitem);
            };
            DataBridge.UIDispatcher.Invoke(action);
        }

        /// <summary>
        /// Метод для сброса таймера,
        /// по которому определяется бездействие
        /// </summary>
        void TimerReset()
        {
            ShutdownTimer.Stop();
            ShutdownTimer.Start();
        }

        /// <summary>
        /// Метод, вызываемый при нажатии кнопки ДОБАВИТЬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Метод, вызываемый при автоматическом закрытии окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShutdownTimer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Метод для получения интервала из файла конфигурации
        /// </summary>
        /// <returns></returns>
        int GetWindowTimeOut()
        {
            //Получение оригинального значения из файла конфигурации
            string strvalue = DataBridge.SettingsFile.GetValue("WindowTimeOut");

            //Объявление переменной
            //для хранения значения интервала для закрытия окна
            int interval;

            //Приведение интервала к типу int
            var result = int.TryParse(strvalue, out interval);

            if (result == true)
            {
                return interval;
            }

            return 60;
        }


    }
}
