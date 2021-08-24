using SortingStantion.Models;
using SortingStantion.TechnologicalObjects;
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
        /// Акцессо для доступа к модели управления
        /// доступом к кнопкам приложения
        /// </summary>
        public ButtonsEnableModel ButtonsEnableModel
        {
            get
            {
                return DataBridge.ButtonsEnableModel;
            }
        }

        /// <summary>
        /// Технологический объект - конвейер
        /// </summary>
        public Conveyor Conveyor
        {
            get
            {
                return DataBridge.Conveyor;
            }
            set
            {
                Conveyor = value;
            }
        }


        /// <summary>
        /// Указатель на экземпляр класса, управляющий 
        /// сканированием продукта
        /// </summary>
        public BoxEngine BoxEngine
        {
            get
            {
                return DataBridge.BoxEngine;
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
