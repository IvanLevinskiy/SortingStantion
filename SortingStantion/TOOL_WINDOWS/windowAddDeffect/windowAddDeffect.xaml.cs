using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SortingStantion.TOOL_WINDOWS.windowAddDeffect
{
    /// <summary>
    /// Логика взаимодействия для windowAddDeffect.xaml
    /// </summary>
    public partial class windowAddDeffect : Window
    {

        /// <summary>
        /// Таймер для автоматического закрытия окна
        /// при бездействии
        /// </summary>
        DispatcherTimer ShutdownTimer;

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
        /// Текущий, просканированый 
        /// GTIN прдукта
        /// </summary>
        string CurrentGTIN
        {
            get
            {
                return currentGTIN;
            }
            set
            {
                currentGTIN = value;
            }
        }
        string currentGTIN;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public windowAddDeffect()
        {
            //Инициализация UI
            InitializeComponent();

            //Подписка на обытие по получению данных от сканера
            DataBridge.Scaner.NewDataNotification += Scaner_NewDataNotification;

            //Количество изделий, отбракованых вручную
            QUANTITY_PRODUCTS_MANUAL_REJECTED = (S7_DWord)device.GetTagByAddress("DB1.DBD28-DWORD");

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
                message = $"Посторонний продукт не может быть добавлен в брак.";
                ShowMessage(message, DataBridge.myBlue);

                //Выход из функции
                return;
            }

            /*
                Продукт в браке
            */
            if (DataBridge.Report.IsDeffect(serialnumber) == true)
            {
                message = $"Продукт номер {serialnumber} уже числиться в браке.";
                ShowMessage(message, DataBridge.myBlue);
                return;
            }

            /*
                Добавление номера продукта в список брака
            */
            DataBridge.Report.AddDeffect(serialnumber);

            /*
                Текст сообщения
            */
            message = $"Продукт номер {serialnumber} перемещен из результата в брак.";

            /*
                Добавление в базу данных (лог) записи
            */
            DataBridge.AlarmLogging.AddMessage(message, Models.MessageType.Info);

            /*
                Добавление сообщения в зону информации
            */
            ShowMessage(message, DataBridge.myOrange);


            //Инкремент счетчика отбракованых изделий вручную
            var value = Convert.ToInt32(QUANTITY_PRODUCTS_MANUAL_REJECTED.Status) + 1;
            QUANTITY_PRODUCTS_MANUAL_REJECTED.Write(value);
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
        /// Метод, вызываемый по клику на кнопку
        /// ДОБАВИТЬ
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

        /// <summary>
        /// Метод для отображения сообщения в зоне информации
        /// </summary>
        /// <param name="message"></param>
        void ShowMessageInformationZone(string message)
        {
            Action action = () =>
            {
                var msgitem = new UserMessage(message, MSGTYPE.INFO);
                DataBridge.MSGBOX.Add(msgitem);
            };
            DataBridge.UIDispatcher.Invoke(action);
        }
    }
}
