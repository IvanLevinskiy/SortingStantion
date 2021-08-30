using S7Communication;
using SortingStantion.Models;
using SortingStantion.TechnologicalObjects;
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

        public Scaner Scaner
        {
            get
            {
                return DataBridge.Scaner;
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
        /// Текущий пользователь
        /// </summary>
        public AuthorizationEngine AccesLevelModel
        {
            get
            {
                return DataBridge.MainAccesLevelModel;
            }
        }

        /// <summary>
        /// Объект, управляющий сообщениями
        /// пользователю
        /// </summary>
        public Message_Engine Message_Engine
        {
            get
            {
                return message_Engine;
            }
            set
            {
                message_Engine = value;
            }
        }
        Message_Engine message_Engine = new Message_Engine();

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
        /// Модель для создания результата операции
        /// </summary>
        public Report ResultOperation
        {
            get
            {
                return DataBridge.Report;
            }
        }

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticClient server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        public SimaticDevice device
        {
            get
            {
                return  server.Devices[0];
            }
        }

        /// <summary>
        /// Указатель на группу, где хранятся все тэгиК
        /// </summary>
        public SimaticGroup group
        {
            get
            {
                return device.Groups[0];
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
        /// Технологический объект - задание
        /// </summary>
        public WorkAssignmentEngine WorkAssignment
        {
            get
            {
                return DataBridge.WorkAssignmentEngine;
            }
            set
            {
                DataBridge.WorkAssignmentEngine = value;
            }
        }

        /// <summary>
        /// Технологический объект - рабочее задание
        /// </summary>

        /// <summary>
        /// Команда принять - завершить
        /// задание
        /// </summary>
        public S7_Boolean TaskTag
        {
            get;
            set;
        }

        /// <summary>
        /// Флаг по которому ПЛК определяет
        /// наличие или отсутсвие связи с ПЛК
        /// (данный флаг надо циклически взводить)
        /// </summary>
        public S7_Boolean ConnectFlag
        {
            get;
            set;
        }


        /// <summary>
        /// Конструктор
        /// </summary>
        public MainViewModel ()
        {
            //Инициализация модели - синхронизатора
            //времени
            CurrentTime = new TimeUpdater();
                        
            //Тэг который сбрасывает дисконект
            ConnectFlag = new S7_Boolean("ConnectFlag", "DB1.DBX0.0", group);

            //Установка таймаута
            server.Timeout = 0;

            //Запуск сервера
            server.Start();

            ////Запуск задачи, взводящий флаг ConnectFlag
            //Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        //ConnectFlag.Write(true);
            //        //Thread.Sleep(500);
            //    }
            //});
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
                    //Вызов окна для авторизации мастера
                    SortingStantion.TOOL_WINDOWS.Authorization.windowAuthorizationSuperUser windowAuthorizationSuperUser = new TOOL_WINDOWS.Authorization.windowAuthorizationSuperUser(false);
                    windowAuthorizationSuperUser.Owner = DataBridge.MainScreen;
                    windowAuthorizationSuperUser.ShowDialog();
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
