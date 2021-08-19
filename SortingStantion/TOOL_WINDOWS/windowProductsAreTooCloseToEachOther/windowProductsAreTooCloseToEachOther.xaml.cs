using System.Windows;

namespace SortingStantion.TOOL_WINDOWS.windowProductsAreTooCloseToEachOther
{
    /// <summary>
    /// Логика взаимодействия для windowProductsAreTooCloseToEachOther.xaml
    /// </summary>
    public partial class windowProductsAreTooCloseToEachOther : Window
    {
        public windowProductsAreTooCloseToEachOther()
        {
            //Инициализация UI
            InitializeComponent();

            //Передача указателя на окно, в центе которого 
            //надо разместить окна
            this.Owner = DataBridge.MainScreen;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
