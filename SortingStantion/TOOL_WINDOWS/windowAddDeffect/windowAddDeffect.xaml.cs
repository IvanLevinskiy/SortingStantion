using S7Communication;
using SortingStantion.Controls;
using System;
using System.Windows;
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
        /// Строка, содержащая серийный номер
        /// продукта, добавляемого в брак
        /// </summary>
        string CurrentSerialNumber
        {
            get
            {
                return currentSerialNumber;
            }
            set
            {
                currentSerialNumber = value;

                //Управление Enable кнопки ДОБАВИТЬ
                btnAddDeffect.IsEnabled = string.IsNullOrEmpty(currentSerialNumber) == false;

                //Сброс таймера отсчета времени бездействия
                ShutdownTimer.Stop();
                ShutdownTimer.Start();
            }
        }
        string currentSerialNumber;

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
            //Текст сообщения в зоне информации
            string message = string.Empty;

            //Разбор данных по полям
            var inputdata = datastring;
            DataBridge.DataSpliter.Split(ref inputdata);

            //Если код не распознан (поля не соответствуют шаблону)
            if (DataBridge.DataSpliter.IsValid == false)
            {
                message = $"Код не распознан";
                ShowMessageInformationZone(message);

                //Выход из функции
                return;
            }

            //Добавление номера продукта в буфер текущего окна
            CurrentSerialNumber = DataBridge.DataSpliter.GetSerialNumber();
            CurrentGTIN = DataBridge.DataSpliter.GetGTIN();

            //Если GTIN не соответсвтвует заданию
            if (CurrentGTIN != DataBridge.WorkAssignmentEngine.GTIN)
            {
                message = $"Посторонний продукт не может быть добавлен в брак.";
                ShowMessageInformationZone(message);
                
                //Стирание старой информации
                CurrentSerialNumber = string.Empty;
                CurrentGTIN = string.Empty;

                //Выход из функции
                return;
            }

            //DataBridge.Report.AddDeffect(CurrentSerialNumber);

            //Формируем сообщение для зоны иноформации
            message = $"Считан продукт GTIN:{CurrentGTIN} SN:{CurrentSerialNumber}";
            ShowMessageInformationZone(message);


        }

        /// <summary>
        /// Метод, вызываемый по клику на кнопку
        /// ДОБАВИТЬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddDeffect_Click(object sender, RoutedEventArgs e)
        {
            //Объявление локальных переменных
            string message = string.Empty;
            UserMessage msg = null; 

            //Проверка - на то, имеется ли продукт в браке
            if (DataBridge.Report.IsDeffect(CurrentSerialNumber) == true)
            {
                //Добавление в базу данных (лог) записи
                message = $"Продукт GTIN:{CurrentGTIN} SN:{CurrentSerialNumber} уже числится в браке";
                msg = new UserMessage(message, DataBridge.myOrange);
                DataBridge.MSGBOX.Add(msg);
                return;
            }

            //Добавление номера продукта в список брака
            DataBridge.Report.AddDeffect(CurrentSerialNumber);

            //Добавление в базу данных (лог) записи
            message = $"Продукт GTIN:{CurrentGTIN} SN:{CurrentSerialNumber} добавлен в список брака";
            DataBridge.AlarmLogging.AddMessage(message, Models.MessageType.Info);

            //Добавление сообщения в зону информации
            msg = new UserMessage(message, DataBridge.myOrange);
            DataBridge.MSGBOX.Add(msg);

            //Стирание серийного номера для того, чтоб
            //через акцессор заблокировать кнопку Добавить и сбросить
            //таймер по которому отсчитывается время бездействия
            CurrentSerialNumber = string.Empty;

            //Инкремент счетчика отбракованых изделий вручную
            var value = Convert.ToInt32(QUANTITY_PRODUCTS_MANUAL_REJECTED.Status) + 1;
            QUANTITY_PRODUCTS_MANUAL_REJECTED.Write(value);


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
