using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Utilites;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace SortingStantion.TechnologicalObjects
{
    /// <summary>
    /// Класс, реализующий модель
    /// конвейера
    /// </summary>
    public class Conveyor : INotifyPropertyChanged
    {

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticServer server
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
                return server.Devices[0];
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
        /// Команда ПУСК-ОСТАНОВКА
        /// </summary>
        public S7BOOL Run
        {
            get;
            set;
        }

        /// <summary>
        /// Тэг, полученный от сканера,
        /// что линия остановлена
        /// </summary>
        public S7BOOL IsStopFromTimerTag
        {
            get;
            set;
        }

        /// <summary>
        /// Флаг, указывающий, что
        /// линия запущена (для View Model)
        /// </summary>
        public bool LineIsRun
        {
            get 
            {
                if (Run.Status is bool? == false)
                {
                    return false;
                }

                return (bool)Run.Status == true;
            }
        }

        /// <summary>
        /// Флаг, указывающий, что
        /// линия остановлена (для View Model)
        /// </summary>
        public bool LineIsStop
        {
            get
            {
                if (Run.Status is bool? == false)
                {
                    return false;
                }

                return (bool)Run.Status == false;
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Conveyor()
        {
            //Тэг для запуска - останова линии
            Run = new S7BOOL("", "DB1.DBX132.0", group);

            //Тэг, указывающий, что линия установлена с учетом задержки
            //на останов
            IsStopFromTimerTag = new S7BOOL("", "DB1.DBX120.1", group);

            //Подпись на событие по изминеию статуса работы
            //линии
            Run.ChangeValue += Run_ChangeValue;

            //Вывод сообщения в окно информации (начальное)
            UserMessage msg = new UserMessage("Линия остановлена", MSGTYPE.WARNING);
            DataBridge.MSGBOX.Add(msg);
        }

        /// <summary>
        /// Метод для запуска конвейера
        /// </summary>
        public void Start()
        {
            //Если статус не является
            //булевым значением - игнорируем обработку
            //команды
            if (Run.Status is bool? == false)
            {
                return;
            }

            //Если конвейер остановлен, запускаем его
            if ((bool?)Run.Status == false)
            {
                //Если задание принято, то запускаем линию
                //иначе уведомляем оператора сообщением
                if (DataBridge.WorkAssignmentEngine.InWork == false)
                {
                    customMessageBox mb = new customMessageBox("Ошибка", "Запуск невозможен, задание не принято в работу");
                    mb.Owner = DataBridge.MainScreen;
                    mb.ShowDialog();

                    return;
                }

                //Запись статуса в ПЛК
                Run.Write(true);

                //Внесение в базу данных сообщения об остановке комплекса
                DataBridge.AlarmLogging.AddMessage("Нажата кнопка СТАРТ. Линия запущена", Models.MessageType.Event);

                return;
            }
        }

        /// <summary>
        /// Команда для запуска
        /// линии
        /// </summary>
        public ICommand StartLineCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Start();
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Метод для остановки конвейера
        /// </summary>
        public void Stop()
        {
            //Если статус не является
            //булевым значением - игнорируем обработку
            //команды
            if (Run.Status is bool? == false)
            {
                return;
            }

            //Если конвейер запущен - останавливаем его
            if ((bool?)Run.Status == true)
            {
                //Запись статуса в ПЛК
                Run.Write(false);

                //Внесение в базу данных сообщения об остановке комплекса
                DataBridge.AlarmLogging.AddMessage("Нажата кнопка СТОП. Линия остановлена", Models.MessageType.Event);

                return;
            }
        }

        /// <summary>
        /// Команда для остановки
        /// линии
        /// </summary>
        public ICommand StopLineCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Stop();
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Метод, вызываемый при изминении
        /// статуса работы линии
        /// </summary>
        /// <param name="obj"></param>
        private void Run_ChangeValue(object oldvalue, object newvalue)
        {
            //Защита от неверных типов данных
            if (newvalue is bool? == false)
            {
                return;
            }

            //Получение статуса работы конвейера
            bool _value = (bool)newvalue;

            //Если конвейер запущен
            if (_value == true)
            {
                Action action = () =>
                {

                    //Вывод сообщения в окно информации
                    UserMessage msg = new UserMessage("Линия запущена", MSGTYPE.SUCCES);
                    DataBridge.MSGBOX.Add(msg);

                    
                };
                DataBridge.UIDispatcher.Invoke(action);
            }

            //Если конвейер остановлен
            if (_value == false)
            {
                Action action = () =>
                {
                    //Вывод сообщения в окно информации
                    UserMessage msg = new UserMessage("Линия остановлена", MSGTYPE.WARNING);
                    DataBridge.MSGBOX.Add(msg);
                };
                DataBridge.UIDispatcher.Invoke(action);
            }

            //уведомление модели представления
            OnPropertyChanged("LineIsRun");
            OnPropertyChanged("LineIsStop");
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
