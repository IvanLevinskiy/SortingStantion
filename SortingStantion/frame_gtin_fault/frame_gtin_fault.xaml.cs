using System.Windows;
using System.Windows.Input;

namespace SortingStantion.frame_gtin_fault
{
    /// <summary>
    /// Логика взаимодействия для frame_gtin_fault.xaml
    /// </summary>
    public partial class frame_gtin_fault : Window
    {
        //string gtin;
        //string barcode;

        public frame_gtin_fault(string gtin, string barcode)
        {
            InitializeComponent();

            //Перенос свойств
            this.gtin.Text = gtin;
            this.barcode.Text = barcode;

            //Формиование сообщения
            //txMessage.Text = $"\tСчитан посторонний продукт\tGTIN: {gtin}\tномер: {barcode}.\nНайдите его ручным сканером и удалите с конвейера.";
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
