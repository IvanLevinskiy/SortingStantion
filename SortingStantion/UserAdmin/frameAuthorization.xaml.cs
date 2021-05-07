using SortingStantion.Utilites;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SortingStantion.UserAdmin
{
    /// <summary>
    /// Логика взаимодействия для frameAuthorization.xaml
    /// </summary>
    public partial class frameAuthorization : Window
    {

        public frameAuthorization()
        {
            InitializeComponent();
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
                    DataBridge.MainAccesLevelModel.Login(tbxLogin.Text, pwbPassword.Password);
                    this.Close();
                },
                (obj) => (true));
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
