using SortingStantion.Models;
using System.Windows.Controls;

namespace SortingStantion.frameSettings
{
    /// <summary>
    /// Логика взаимодействия для frameSettings.xaml
    /// </summary>
    public partial class frameSettings : UserControl
    {

        /// <summary>
        /// Модель для управления экранами
        /// </summary>
        public ScreenEngine ScreenEngine
        {
            get
            {
                return DataBridge.ScreenEngine;
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public frameSettings()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
