using SortingStantion.TOOL_WINDOWS.windowAddDeffect;
using SortingStantion.Utilites;
using System.ComponentModel;
using System.Windows.Input;

namespace SortingStantion.Models
{
    /// <summary>
    /// Движок для управления экранами
    /// </summary>
    public class ScreenEngine : INotifyPropertyChanged
    {

        /// <summary>
        /// Главный экран
        /// </summary>
        frameMain.frameMain frameMain = new SortingStantion.frameMain.frameMain();

        /// <summary>
        /// Экран настроек
        /// </summary>
        frameSettings.frameSettings frameSettings = new SortingStantion.frameSettings.frameSettings();

        /// <summary>
        /// Текущий экран
        /// </summary>
        object currentScreen;
        public object CurrentScreen
        {
            get
            {
                return currentScreen;
            }
            set
            {
                currentScreen = value;
                OnPropertyChanged("CurrentScreen");
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public ScreenEngine()
        {
            CurrentScreen = frameMain;
        }

        /// <summary>
        /// Команда открытия главного
        /// экрана
        /// </summary>
        public ICommand OpenMainScreenCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CurrentScreen = frameMain;
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда открытия экрана
        /// настроек
        /// </summary>
        public ICommand OpenSettingsScreenCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CurrentScreen = frameSettings;
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для выключения комплекса
        /// </summary>
        public ICommand ShutdownCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    frameSettings.windowExit windowExit = new frameSettings.windowExit();
                    windowExit.Owner = DataBridge.MainScreen;
                    windowExit.ShowDialog();

                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для добавления нового короба
        /// (при нажатии на клавишу Добавить)
        /// </summary>
        public ICommand AddingBoxCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    windiwAddingBox.windiwAddingBox windiwAddingBox = new windiwAddingBox.windiwAddingBox();
                    windiwAddingBox.Owner = DataBridge.MainScreen;
                    windiwAddingBox.ShowDialog();
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для открытия окна ДОБАВТЬ ДЕФФЕКТ
        /// </summary>
        public ICommand AddingDeffectCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    windowAddDeffect windowAddDeffect = new windowAddDeffect();
                    windowAddDeffect.Owner = DataBridge.MainScreen;
                    windowAddDeffect.ShowDialog();
                },
                (obj) => (true));
            }
        }

        #region Реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }

        }
        #endregion
    }
}
