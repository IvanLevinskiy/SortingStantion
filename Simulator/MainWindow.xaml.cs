using System.Windows;
using S7Communication;

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
        }
    }
}
