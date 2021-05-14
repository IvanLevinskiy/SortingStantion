using S7Communication;
using Simatic;
using SortingStantion.Models;
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
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticServer server = DataBridge.server;

        /// <summary>
        /// Команда ПУСК-ОСТАНОВКА
        /// </summary>
        public S7BOOL Run
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

            //Инициализация устройства
            var device = new SimaticDevice("192.168.0.1", CpuType.S71200, slot:1, rack:0);

            //Группа тэгов
            var group = new SimaticGroup("");

            //Тэг который сбрасывает дисконект
            ConnectFlag = new S7BOOL("ConnectFlag", "DB1.DBX0.0", group);

           

            Run = new S7BOOL("", "DB1.DBX18.0", group);


            group.AddTag(Run);
            device.AddGroup(group);
            server.AddDevice(device);

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
        /// Команда для открытия окна авторизации
        /// </summary>
        public ICommand Start
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Run.Write(true);
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для открытия окна авторизации
        /// </summary>
        public ICommand Stop
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Run.Write(false);
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
