using SortingStantion.Utilites;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SortingStantion.Controls
{
    /// <summary>
    /// Типы сообщений
    /// </summary>
    public enum MSGTYPE
    {
        ERROR, WARNING, INFO, SUCCES
    }

    /// <summary>
    /// Логика взаимодействия для MSG.xaml
    /// </summary>
    public partial class MSG : UserControl
    {

        /// <summary>
        /// Текст сообщения
        /// </summary>
        string message = string.Empty;
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                OnPropertyChanged("Message");
            }
        }


        /// <summary>
        /// Время возникновления сообщения
        /// </summary>
        string datetime = string.Empty;
        public string Datetime
        {
            get
            {
                return datetime;
            }
            set
            {
                datetime = value;
                OnPropertyChanged("Datetime");
            }
        }


        public Brush Brush
        {
            get;
            set;
        }

        /// <summary>
        /// Тип сообщения
        /// </summary>
        MSGTYPE type = MSGTYPE.ERROR;
        public MSGTYPE Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="message"></param>
        /// <param name=""></param>
        public MSG(string message, MSGTYPE type)
        {
            InitializeComponent();

            DataContext = this;

            //Инициализация полей
            Message = message;
            Type = type;
            Datetime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");

            //Определение цвета окна
            Brush = MSGTYPE_TO_BRUSH(Type);
        }

        /// <summary>
        /// Перевод типа сообщения в цвет кисти
        /// </summary>
        /// <returns></returns>
        SolidColorBrush MSGTYPE_TO_BRUSH(MSGTYPE type)
        {
            if (type == MSGTYPE.WARNING)
            {
                return new SolidColorBrush(Colors.Orange);
            }

            if (type == MSGTYPE.SUCCES)
            {
                //Зеленый цвет
                return new SolidColorBrush(Color.FromArgb(0xFF, 0x7D, 0xD2, 0x94));
            }

            if (type == MSGTYPE.ERROR)
            {
                return new SolidColorBrush(Color.FromArgb(0xFF, 0xDB, 0x49, 0x69));
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        /// <summary>
        /// Команда для удаления текущего контрола
        /// </summary>
        public ICommand DeleteCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    DataBridge.MSGBOX.Remove(this);
                },
                (obj) => (true));
            }
        }


        #region РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА INotifyPropertyChanged
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
