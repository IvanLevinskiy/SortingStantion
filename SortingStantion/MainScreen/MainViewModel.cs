using SortingStantion.Models;
using SortingStantion.UserAdmin;
using SortingStantion.Utilites;
using System.ComponentModel;
using System.Windows.Input;

namespace SortingStantion.MainScreen
{
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Модель для обновления отбражаемого на главном экране
        /// времени
        /// </summary>
        public TimeUpdater CurrentTime
        {
            get;
            set;
        }

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public AccesLevelModel AccesLevelModel
        {
            get
            {
                return DataBridge.MainAccesLevelModel;
            }
        }


        /// <summary>
        /// Конструктор
        /// </summary>
        public MainViewModel ()
        {
            //Инициализация модели - синхронизатора
            //времени
            CurrentTime = new TimeUpdater();

            //Инициализация модели пользователей
            DataBridge.MainAccesLevelModel = new AccesLevelModel();
        }

        /// <summary>
        /// Команда для открытия окна авторизации
        /// </summary>
        public ICommand AuthorizationCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    frameAuthorization frameAuthorization = new frameAuthorization();
                    frameAuthorization.ShowDialog();
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
