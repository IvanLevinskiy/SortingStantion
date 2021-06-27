using S7Communication;
using System.Windows;

namespace SortingStantion.TOOL_WINDOWS.windowGtinFault
{
    /// <summary>
    /// Логика взаимодействия для frame_gtin_fault.xaml
    /// </summary>
    public partial class windowGtinFault : Window
    {

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticServer server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        public SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Указатель на группу, где хранятся все тэгиК
        /// </summary>
        public SimaticGroup group
        {
            get
            {
                return device.Groups[0];
            }
        }

        /// <summary>
        /// Тэг GTIN
        /// </summary>
        public S7_STRING GTIN
        {
            get;
            set;
        }

        /// <summary>
        /// Тэг ID
        /// </summary>
        public S7_STRING SERIALNUMBER
        {
            get;
            set;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public windowGtinFault()
        {
            InitializeComponent();
            DataContext = this;

            GTIN = (S7_STRING)device.GetTagByAddress("DB1.DBD416-STR14");
            SERIALNUMBER = (S7_STRING)device.GetTagByAddress("DB1.DBD432-STR6");
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="gtin"></param>
        /// <param name="barcode"></param>
        public windowGtinFault(string gtin, string barcode)
        {
            InitializeComponent();

            //Подписка на события
            this.Closing += Window_Closing;
        }

        /// <summary>
        /// Метод, вызываемый при клике по кнопке - ОТМЕНА
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DataBridge.AlarmsEngine.al_1.Write(false);
            this.Closing -= Window_Closing;
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
