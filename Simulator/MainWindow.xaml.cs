using System.Windows;

namespace Simulator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            this.Loaded += (s, e) =>
            {
                DataBridge.Server.Start();
            };

            this.Closing += (s, e) =>
            {
                  simmodeBtn.IsChecked = false;
            };
        }
    }
}
