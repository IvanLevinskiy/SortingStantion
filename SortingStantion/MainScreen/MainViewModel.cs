using S7Communication;
using SortingStantion.Models;
using SortingStantion.S7Extension;
using SortingStantion.TechnologicalObjects;
using SortingStantion.UserAdmin;
using SortingStantion.Utilites;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
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
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticServer server
        {
            get
            {
                return DataBridge.server;
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
        /// Технологический объект - рабочее задание
        /// </summary>

        /// <summary>
        /// Команда принять - завершить
        /// задание
        /// </summary>
        public S7BOOL TaskTag
        {
            get;
            set;
        }

        /// <summary>
        /// Флаг по которому ПЛК определяет
        /// наличие или отсутсвие связи с ПЛК
        /// (данный флаг надо циклически взводить)
        /// </summary>
        public S7BOOL ConnectFlag
        {
            get;
            set;
        }

       

        Item CurrentItem
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

            //Инициализация модели пользователей
            DataBridge.MainAccesLevelModel = new AccesLevelModel();
                        
            //Тэг который сбрасывает дисконект
            ConnectFlag = new S7BOOL("ConnectFlag", "DB1.DBX0.0", group);

            //Тэг для принятия - завершения задания
            TaskTag = new S7BOOL("", "DB1.DBX182.0", group);

            //Запуск сервера
            server.Start();


            //Запуск задачи, взводящий флаг ConnectFlag
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    ConnectFlag.Write(true);
                    Thread.Sleep(500);
                }
            });
        }

      

        /// <summary>
        /// Команда для запуска - остановки
        /// линии
        /// </summary>
        public ICommand AcceptTaskCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //Если статус не является
                    //булевым значением - игнорируем обработку
                    //команды
                    if (TaskTag.Status is bool? == false)
                    {
                        return;
                    }

                    if ((bool?)TaskTag.Status == true)
                    {
                        //Запись статуса в ПЛК
                        TaskTag.Write(false);
                        return;
                    }

                    if ((bool?)TaskTag.Status == false)
                    {
                        //Запись статуса в ПЛК
                        TaskTag.Write(true);
                        return;
                    }
                },
                (obj) => (true));
            }
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
