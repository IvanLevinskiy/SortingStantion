using SortingStantion.Utilites;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SortingStantion.UserAdmin
{
    /// <summary>
    /// Логика взаимодействия для frameAuthorization.xaml
    /// </summary>
    public partial class frameAuthorization : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Имена пользователей
        /// </summary>
        public ObservableCollection<User> Users
        {
            get
            { 
            return DataBridge.MainAccesLevelModel.Users;
            }
            set
            {
                DataBridge.MainAccesLevelModel.Users = value;
            }
        }

        /// <summary>
        /// Выбраный в combobox пользователь
        /// </summary>
        User selectedUser = null;
        public User SelectedUser
        {
            get
            {
                return selectedUser;
            }
            set
            {
                selectedUser = value;
                OnPropertyChanged("SelectedUser");
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public frameAuthorization()
        {
            //иИнициализация UI
            InitializeComponent();

            //Передача контекста данных
            DataContext = this;
        }


        /// <summary>
        /// Команда авторизации пользователя
        /// </summary>
        public ICommand AuthorizationCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    DataBridge.MainAccesLevelModel.Login(SelectedUser.Name, pwbPassword.Password);
                    this.Close();
                },
                (obj) => (SelectedUser != null));
            }
        }

        #region РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion


    }
}
