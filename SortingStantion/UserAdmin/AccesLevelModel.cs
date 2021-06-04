using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;

namespace SortingStantion.UserAdmin
{
    /// <summary>
    /// Перечисление уровней доступа
    /// </summary>
    public enum AccesLevels
    {
        [Description("Мастер")]
        Мастер = 0,

        [Description("Наладчик")]
        Наладчик = 1,
    }

    /// <summary>
    /// Класс, реализующий разграничение 
    /// прав пользователя
    /// </summary>
    public class AccesLevelModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Коллекция пользователей
        /// </summary>
        public ObservableCollection<User> Users
        {
            get;
            set;
        }

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public User CurrentUser
        {
            get
            {
                return currentUser;
            }
            set
            {
                currentUser = value;
                OnPropertyChanged("CurrentUser");
            }
        }
        User currentUser = new User();

        /// <summary>
        /// Отображаемое уровнь - имя
        /// текущего пользователя
        /// на модели представления
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (CurrentUser == null)
                {
                    return "Мастер:";
                }

                return $"{CurrentUser.AccesLevel}:\n{CurrentUser.Name}";
            }
        }

        /// <summary>
        /// Событие, возникающее при смене
        /// пользователя
        /// </summary>
        public event Action<int> ChangeUser;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public AccesLevelModel()
        {
            //Инициализация текущего пользователя
            CurrentUser = null;

            //Инициализация коллекции пользователей
            Users = new ObservableCollection<User>();

            //Загружаем из файла пользователей
            LoadFromXML();
        }

        /// <summary>
        /// Метод для загрузки данных из файла
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool LoadFromXML()
        {
            //Создание экземпляра документа
            XmlDocument xDoc = new XmlDocument();

            //Заргузка из файла
            try
            {
                xDoc.Load(@"AppData\Users.xml");
            }
            catch (Exception ex)
            {
                return false;
            }

            //Получение корневого элемента
            XmlElement root = xDoc.DocumentElement;

            //Обход всех узлов в корневом элементе
            foreach (XmlNode userNode in root)
            {
                if (userNode.Name.ToUpper() == "USER")
                {
                    var user = new User(userNode);
                    this.Users.Add(user);
                }
            }

            return true;
        }


        /// <summary>
        /// Метод для авторизации
        /// </summary>
        public bool Login(string login, string Password)
        {
            foreach (var user in Users)
            {
                //Если имя пользователя не соответствует
                //имени пользователя из коллеции
                //переходим к следующему пользователю
                if (user.Name.ToUpper() != login.ToUpper())
                {
                    continue;
                }

                ////Если пользователь валидный
                ////извещаем подписчиков
                if (user.Password == Password)
                {
                    CurrentUser = user;
                    OnPropertyChanged("DisplayName");
                    ChangeUser?.Invoke((int)CurrentUser.AccesLevel);
                    return true;
                }
            }

            return false;
        }




        /// <summary>
        /// Выход из логина
        /// </summary>
        public void Logout()
        {
            CurrentUser = new User();
            //ChangeUser?.Invoke((int)CurrentUser.AccesLevel);
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
