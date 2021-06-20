using SortingStantion.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SortingStantion.TOOL_WINDOWS.windiwInformation
{
    /// <summary>
    /// Логика взаимодействия для windiwInformation.xaml
    /// </summary>
    public partial class windiwInformation : Window
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
        public windiwInformation()
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

            //Добавление номера продукта в буфер текущего окна
            bool isvalid = DataBridge.DataSpliter.IsValid;
            CurrentSerialNumber = DataBridge.DataSpliter.GetSerialNumber();
            CurrentGTIN = DataBridge.DataSpliter.GetGTIN();
            DataBridge.Report.AddBox(CurrentSerialNumber);

            // Сообщение из зоны информации
            string message = string.Empty;

            //Посторонний продукт
            if (isvalid == false)
            {
                message = "Код другого продукта";
                ShowMessage(message);
                return;
            }

            //Посторонний GTIN
            if (DataBridge.WorkAssignmentEngine.GTIN != CurrentGTIN)
            {
                message = "Код другого продукта";
                ShowMessage(message);
                return;
            }

            //Посторонний GTIN
            if (DataBridge.WorkAssignmentEngine.GTIN != CurrentGTIN)
            {
                message = "Код другого продукта";
                ShowMessage(message);
                return;
            }

            //Продукт в браке
            if (DataBridge.Report.IsDeffect(CurrentSerialNumber) == true)
            {
                message = $"Продукт «{CurrentSerialNumber}» в браке.";
                ShowMessage(message);
                return;
            }

            //Продукт в результате
            if (DataBridge.Report.AsAResult(CurrentSerialNumber) == true)
            {
                message = $"Продукт «{CurrentSerialNumber}» в результате.";
                ShowMessage(message);
                return;
            }

            //Продукт «s/n» доступен для сериализации
            message = $"Продукт «{CurrentSerialNumber}» доступен для сериализации.";
            ShowMessage(message);



        }

        /// <summary>
        /// Метод для отображения сообщения в хоне информации
        /// </summary>
        /// <param name="message"></param>
        void ShowMessage(string message)
        {
            Action action = () =>
            {
                var msgitem = new UserMessage(message, MSGTYPE.INFO);
                DataBridge.MSGBOX.Add(msgitem);
            };
            DataBridge.UIDispatcher.Invoke(action);
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
