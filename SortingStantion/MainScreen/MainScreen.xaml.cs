using System.Windows;

namespace SortingStantion.MainScreen
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainScreen : Window
    {
        public MainScreen()
        {
            InitializeComponent();

            //ПЕредача экземпляра окна
            DataBridge.MainScreen = this;

            //Передача  UI Dispatcher
            DataBridge.UIDispatcher = this.Dispatcher;
        }

        /// <summary>
        /// Метод для закрытия окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch
            { 
            
            }
        }
    }
}
