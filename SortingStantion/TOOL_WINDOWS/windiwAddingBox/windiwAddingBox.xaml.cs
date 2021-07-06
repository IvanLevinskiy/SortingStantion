using SortingStantion.Controls;
using System;
using System.Windows;
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
                //Запоминание серийного номера
                currentSerialNumber = value;

                //Управление Enable кнопки ДОБАВИТЬ
                //btnAddBox.IsEnabled = string.IsNullOrEmpty(currentSerialNumber) == false;

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
            //Разбор данных по полям
            var inputdata = datastring;
            DataBridge.DataSpliter.Split(ref inputdata);

            Action action;
            string message;

            /*
                НЕВЕРНЫЙ КОД (НЕ СОВПАДАЕТ СТРУКТУРА КОДА) 
            */
            if (DataBridge.DataSpliter.IsValid == false)
            {
                //Формирование сообщения
                message = $"Код: {DataBridge.DataSpliter.SourseData} не распознан";

                //Вывод информации в зоне информации
                action = () =>
                {
                    var msgitem = new UserMessage(message, MSGTYPE.INFO);
                    DataBridge.MSGBOX.Add(msgitem);
                };
                DataBridge.UIDispatcher.Invoke(action);
                
                //Выход
                return;
            }

            /*
                НЕВЕРНЫЙ GTIN 
            */
            if (DataBridge.DataSpliter.GTIN != DataBridge.WorkAssignmentEngine.GTIN)
            {
                //Формирование сообщения
                message = $"Посторонний продукт не может быть добавлен в результат.";

                //Вывод информации в зоне информации
                action = () =>
                {
                    var msgitem = new UserMessage(message, MSGTYPE.INFO);
                    DataBridge.MSGBOX.Add(msgitem);
                };
                DataBridge.UIDispatcher.Invoke(action);

                //Выход
                return;
            }

            /*
                ПРОДУКТ УЖЕ СОДЕРЖИТСЯ В РЕЗУЛЬТАТЕ
            */
            var asAResult = DataBridge.Report.AsAResult(DataBridge.DataSpliter.SerialNumber);
            if (asAResult == true)
            {
                //Формирование сообщения
                message = $"Продукт «{DataBridge.DataSpliter.SerialNumber}» уже есть в результате.";

                //Вывод информации в зоне информации
                action = () =>
                {
                    var msgitem = new UserMessage(message, MSGTYPE.INFO);
                    DataBridge.MSGBOX.Add(msgitem);
                };
                DataBridge.UIDispatcher.Invoke(action);

                //Выход
                return;
            }


            /*
              Добавление номера продукта в буфер текущего окна
            */
            CurrentSerialNumber = DataBridge.DataSpliter.GetSerialNumber();
            CurrentGTIN = DataBridge.DataSpliter.GetGTIN();
            DataBridge.Report.AddBox(CurrentSerialNumber);

            //Добавление кода в результа
            DataBridge.Report.AddBox(CurrentSerialNumber);

            //Выводим сообщение в зоне иноформации
            message = $"Считан продукт GTIN:{CurrentGTIN} SN:{CurrentSerialNumber}";

            action = () =>
            {
                var msgitem = new UserMessage(message, MSGTYPE.INFO);
                DataBridge.MSGBOX.Add(msgitem);
            };
            DataBridge.UIDispatcher.Invoke(action);

            //Стирание кода из буфера для того, чтоб
            //заблокировать кнопку Добавить
            CurrentSerialNumber = string.Empty;

        }

        /// <summary>
        /// Метод, вызываемый при нажатии кнопки ДОБАВИТЬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddBoxClick(object sender, RoutedEventArgs e)
        {
            //Добавление кода в результа
            DataBridge.Report.AddBox(CurrentSerialNumber);

            //Выводим сообщение в зоне иноформации
            var message = $"Считан продукт GTIN:{CurrentGTIN} SN:{CurrentSerialNumber}";

            Action action = () =>
            {
                var msgitem = new UserMessage(message, MSGTYPE.INFO);
                DataBridge.MSGBOX.Add(msgitem);
            };
            DataBridge.UIDispatcher.Invoke(action);

            //Стирание кода из буфера для того, чтоб
            //заблокировать кнопку Добавить
            CurrentSerialNumber = string.Empty;
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
