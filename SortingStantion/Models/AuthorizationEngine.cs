using SortingStantion.Controls;
using SortingStantion.UserAdmin;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml;

namespace SortingStantion.Models
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
    public class AuthorizationEngine : INotifyPropertyChanged
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
        public event Action<int, User> ChangeUser;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public AuthorizationEngine()
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
        public bool Login(string login, string Password, bool RepeatIgnore = true)
        {
            //Локальные переменные для вывода сообщения
            //в зоне информации
            SolidColorBrush brush;
            UserMessage messageItem;

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
                ////извещаем подписчиков и делаем запись в базу данных
                if (user.Password == Password)
                {
                    //Если текущий пользователь тот же, который был,
                    //пишем
                    if (currentUser == user && RepeatIgnore == false)
                    {
                        brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xDB, 0x49, 0x69));
                        messageItem = new Controls.UserMessage("Пользователь уже авторизован", brush);
                        DataBridge.MSGBOX.Add(messageItem);

                        return false;
                    }

                    //В случае успешной авторизации
                    CurrentUser = user;
                    OnPropertyChanged("DisplayName");
                    ChangeUser?.Invoke((int)CurrentUser.AccesLevel, user);

                    //Запись в базу данных
                    var message = $"Авторизован новый пользователь: {user.Name} с уровнем доступа: {user.AccesLevel.ToString()}";
                    DataBridge.AlarmLogging.AddMessage(message, Models.MessageType.ChangeUser);

                    //Выводим сообщение об успешной авторизации
                    brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xDB, 0x49, 0x69));
                    messageItem = new Controls.UserMessage(message, MSGTYPE.INFO);
                    DataBridge.MSGBOX.Add(messageItem);

                    return true;
                }
            }

            //Выводим сообщение, что при ошибке возникла ошибка авторизации
            brush =  new SolidColorBrush(Color.FromArgb(0xFF, 0xDB, 0x49, 0x69));
            messageItem = new Controls.UserMessage("Ошибка авторизации", brush);
            DataBridge.MSGBOX.Add(messageItem);

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
