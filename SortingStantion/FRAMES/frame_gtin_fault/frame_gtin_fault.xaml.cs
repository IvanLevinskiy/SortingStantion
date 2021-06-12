using S7Communication;
using System.Windows;
using System.Windows.Input;

namespace SortingStantion.frame_gtin_fault
{
    /// <summary>
    /// Логика взаимодействия для frame_gtin_fault.xaml
    /// </summary>
    public partial class frame_gtin_fault : Window
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
        public frame_gtin_fault()
        {
            InitializeComponent();
            DataContext = this;

            GTIN = (S7_STRING)device.GetTagByAddress("DB1.DBD416-STR14");
            SERIALNUMBER = (S7_STRING)device.GetTagByAddress("DB1.DBD432-STR6");
        }

        public frame_gtin_fault(string gtin, string barcode)
        {
            InitializeComponent();

            //Перенос свойств

        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// Метод, вызываемый при клике по кнопке - ОТМЕНА
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DataBridge.AlarmsEngine.al_1.Write(false);
            this.Close();
        }
    }
}
