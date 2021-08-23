using Newtonsoft.Json;
using S7Communication;
using SortingStantion.Controls;
using SortingStantion.TOOL_WINDOWS.windowClearCollectionRequest;
using SortingStantion.Utilites;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace SortingStantion.Models
{
    /// <summary>
    /// Объект, осуществляющий управлением
    /// заданиями
    /// </summary>
    public class WorkAssignmentEngine : INotifyPropertyChanged
    {
        #region SIMATIC СУЩНОСТИ 

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticServer server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Указатель на группу, где хранятся все тэгиК
        /// </summary>
        SimaticGroup group
        {
            get
            {
                return device.Groups[0];
            }
        }

        #endregion

      
        #region SIMATIC ТЭГИ

        /// <summary>
        /// Тэг, отвечающий за Принять - завершить задание
        /// </summary>
        S7_Boolean IN_WORK_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Уникальный идентификатор задания.
        /// </summary>
        S7_String TASK_ID_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Номер GTIN. (14 символов)
        /// </summary>
        S7_String GTIN_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Наименование продукта (UTF-8)
        /// </summary>
        S7_String PRODUCT_NAME_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Номер производственной серии (до 20 символов) 
        /// </summary>
        S7_String LOT_NO_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Кол-во продуктов в коробе. 
        /// </summary>
        S7_Word NUM_PACKS_IN_BOX_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Ожидаемое количество продуктов в серии 
        /// (определяется по заданию на производство серии) 
        /// </summary>
        S7_Word NUM_PACKS_IN_SERIES_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Общее количество изделий
        /// </summary>
        S7_DWord QUANTITY_WORKSPACE
        {
            get;
            set;
        }

        /// <summary>
        /// Счетчик коробов
        /// </summary>
        S7_DWord QUANTITY_BOXS
        {
            get;
            set;
        }

        /// <summary>
        /// Количество изделий, 
        /// отбракованых отбраковщиком автоматически
        /// </summary>
        S7_DWord QUANTITY_WORKSPACE_AUTO_REJECTED
        {
            get;
            set;
        }

        /// <summary>
        /// Количество изделий, отбракованых вручную
        /// </summary>
        S7_DWord QUANTITY_WORKSPACE_MANUAL_REJECTED
        {
            get;
            set;
        }

        /// <summary>
        /// Счетчик дефектного продукта
        /// </summary>
        S7_DWord DEFECT_COUNTER
        {
            get;
            set;
        }

        /// <summary>
        /// Счетчик повторов
        /// </summary>
        S7_DWord REPEAT_COUNTER
        {
            get;
            set;
        }



        /// <summary>
        /// Метод для инициализации данных
        // хранящихся в ПЛК
        /// </summary>
        void PlcDataInit()
        {
            //Инициализация тэгов
            IN_WORK_TAG = (S7_Boolean)device.GetTagByAddress("DB1.DBX148.0");
            IN_WORK_TAG.ChangeValue += (oldvalue, newvalue) =>
            {
                InWork = (bool)newvalue;
                InNotWork = !InWork;
            };

            //ID задания
            TASK_ID_TAG = (S7_String)device.GetTagByAddress("DB1.DBD150-STR40");
            TASK_ID_TAG.ChangeValue += (oldvalue, newvalue) =>
            {
                TaskID = TASK_ID_TAG.StatusText;
            };

            //GTIN
            GTIN_TAG = (S7_String)device.GetTagByAddress("DB1.DBD192-STR40");
            GTIN_TAG.ChangeValue += (oldvalue, newvalue) =>
            {
                GTIN = GTIN_TAG.StatusText;
            };

            ///Номер производственной серии
            LOT_NO_TAG = (S7_String)device.GetTagByAddress("DB1.DBD318-STR40");
            LOT_NO_TAG.ChangeValue += (oldvalue, newvalue) =>
            {
                Lot_No = LOT_NO_TAG.StatusText;
            };

            //Наименорвание продукта
            PRODUCT_NAME_TAG = (S7_String)device.GetTagByAddress("DB1.DBD234-STR82");
            PRODUCT_NAME_TAG.ChangeValue += (oldvalue, newvalue) =>
            {
                Product_Name = PRODUCT_NAME_TAG.StatusText;
            };

            NUM_PACKS_IN_BOX_TAG = (S7_Word)device.GetTagByAddress("DB1.DBW362-WORD");
            NUM_PACKS_IN_SERIES_TAG = (S7_Word)device.GetTagByAddress("DB1.DBW364-WORD");

            QUANTITY_WORKSPACE = (S7_DWord)device.GetTagByAddress("DB1.DBD16-DWORD");
            QUANTITY_BOXS = (S7_DWord)device.GetTagByAddress("DB1.DBD20-DWORD");
            QUANTITY_WORKSPACE_AUTO_REJECTED = (S7_DWord)device.GetTagByAddress("DB1.DBD24-DWORD");
            QUANTITY_WORKSPACE_MANUAL_REJECTED = (S7_DWord)device.GetTagByAddress("DB1.DBD28-DWORD");
            DEFECT_COUNTER = (S7_DWord)device.GetTagByAddress("DB1.DBD30-DWORD");
            REPEAT_COUNTER = (S7_DWord)device.GetTagByAddress("DB1.DBD36-DWORD");
            
        }


        #endregion

        /// <summary>
        /// Флаг возвращает или задает
        /// принято ли задание
        /// </summary>
        public bool InWork
        {
            get
            {
                //Возвращение значения тэга
                return inWorkFeedBack;
            }
            set
            {
                inWorkFeedBack = value; ;
                OnPropertyChanged("InWork");
            }
        }
        bool inWorkFeedBack = false;

        /// <summary>
        /// Флаг, указывающий, 
        /// что задание не принято в работу
        /// </summary>
        public bool InNotWork
        {
            get
            {
                return inNotWork;
            }
            private set
            {
                inNotWork = value;
                OnPropertyChanged("InNotWork");
            }
        }
        bool inNotWork = false;

        /// <summary>
        /// Уникальный идентификатор задания.
        /// </summary>
        public string TaskID
        {
            get
            {
                return taskID;
            }
            set
            {
                taskID = value;
                TASK_ID_TAG.Write(value);
                OnPropertyChanged("TaskID");
            }
        }
        string taskID = string.Empty;

        /// <summary>
        /// Номер GTIN. (14 символов)
        /// </summary>
        public string GTIN
        {
            get
            {
                return gtin;
            }
            set
            {
                gtin = value;
                GTIN_TAG.Write(gtin);
                OnPropertyChanged("GTIN");
            }
        }
        string gtin = string.Empty;

        /// <summary>
        /// Наименование продукта (UTF-8)
        /// </summary>
        public string Product_Name
        {
            get
            {
                return product_Name;
            }
            set
            {
                product_Name = value;
                PRODUCT_NAME_TAG.Write(product_Name);
                OnPropertyChanged("Product_Name");
            }
        }
        string product_Name = string.Empty;

        /// <summary>
        /// Номер производственной серии (до 20 символов) 
        /// </summary>
        public string Lot_No
        {
            get
            {
                return lot_No;
            }
            set
            {
                lot_No = value;
                LOT_NO_TAG.Write(lot_No);
                OnPropertyChanged("Lot_No");
            }
        }
        string lot_No = string.Empty;

        /// <summary>
        /// Коллекция рабочих заданий
        /// </summary>
        public ObservableCollection<WorkAssignment> WorkAssignments
        {
            get;
            set;
        }

        /// <summary>
        /// Выбранное задание
        /// </summary>
        public WorkAssignment SelectedWorkAssignment
        {
            get
            {
                return selectedWorkAssignment;
            }
            set
            {
                selectedWorkAssignment = value;
            }
        }
        WorkAssignment selectedWorkAssignment;

        /// <summary>
        /// Событие, генерируемое при принятии
        /// нового задания
        /// </summary>
        public event Action<WorkAssignment> WorkOrderAcceptanceNotification;

        /// <summary>
        /// Событие, генерируемое при завершении
        /// рабочего задания
        /// </summary>
        public event Action<WorkAssignment> WorkOrderCompletionNotification;

        /// <summary>
        /// Событие, генерируемое при получении нового
        /// рабочего задания от L3
        /// </summary>
        public event Action<WorkAssignment> NewWorkOrderHasArrivedNotification;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public WorkAssignmentEngine()
        {
            //Инициализация коллекции с рабочими заданиями
            WorkAssignments = new ObservableCollection<WorkAssignment>();

            //Инициализация переменных ПЛК
            PlcDataInit();

            //Запуск задачи по прослушиванию
            //http
            Task.Factory.StartNew(() =>
            {
                try
                {
                    StartListener();
                }
                catch (System.Net.HttpListenerException ex)
                {
                    UserMessage messageItem = new Controls.UserMessage(ex.Message, DataBridge.myRed);
                    DataBridge.MSGBOX.Add(messageItem);
                }
               
            });

            //Подгрузка текущего задания
            device.FirstScan += () =>
            {
                if (InWork == true)
                {
                    var wA = new WorkAssignment();
                    wA.gtin = GTIN;
                    wA.ID = TaskID;
                    wA.productName = Product_Name;
                    wA.numРacksInBox = int.Parse(NUM_PACKS_IN_BOX_TAG.StatusText);
                    wA.numPacksInSeries = int.Parse(NUM_PACKS_IN_SERIES_TAG.StatusText);

                    SelectedWorkAssignment = wA;
                    WorkOrderAcceptanceNotification?.Invoke(wA);
                }
            };
        }

       
        bool CheckTask(WorkAssignment workAssignment)
        {
            //Результат проверки
            bool result = true;

            Action action = null;
            string messege = string.Empty;


            if (this.InWork == false)
            {
                //Проверка ID
                if (string.IsNullOrEmpty(workAssignment.ID) == true)
                {
                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: ID", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Проверка GTIN
                if (string.IsNullOrEmpty(workAssignment.gtin) == true)
                {
                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: GTIN", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Проверка lineNum
                if (string.IsNullOrEmpty(workAssignment.lineNum) == true)
                {
                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: LineNum", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Проверка lotNo
                if (string.IsNullOrEmpty(workAssignment.lotNo) == true)
                {

                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: LotNo", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Проверка productName
                if (string.IsNullOrEmpty(workAssignment.productName) == true)
                {
                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: ProductName", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Проверка numPacksInSeries
                if (workAssignment.numPacksInSeries == 0)
                {
                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: numPacksInSeries", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Проверка numРacksInBox
                //if (workAssignment.numРacksInBox == 0)
                //{
                //    return false;
                //}

                //Ели проверка пройдена
                return true;
            }

            action = () =>
            {
                UserMessage messageItem = new Controls.UserMessage("Новое задание не может быть принято в работу поскольку текущее задание не завершено", DataBridge.myRed);
                DataBridge.MSGBOX.Add(messageItem);
            };
            DataBridge.UIDispatcher.Invoke(action);

            return false;

            return false;
        }

        /// <summary>
        /// Команда - принять задание
        /// </summary>
        public ICommand AcceptTaskCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //Если в коллекции заданий заданий нет -
                    //пишем ошибку - "Задание не может быть принято в работу"
                    if (WorkAssignments.Count == 0)
                    {
                        UserMessage messageItem = new Controls.UserMessage("В буфере нет заданий, которые могут быть приняты в работу", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                        return;
                    }

                    //Если задание не принято в работу
                    if (InWork == false)
                    {
                        //Переночим задание в выбраное задание
                        SelectedWorkAssignment = WorkAssignments[0];

                        //Удаление принятого задания из списка заданий
                        WorkAssignments.Remove(SelectedWorkAssignment);


                        //Вызываем окно авторизации
                        SortingStantion.UserAdmin.frameAuthorization frameAuthorization = new SortingStantion.UserAdmin.frameAuthorization();
                        frameAuthorization.ShowDialog();

                        //Если результат авторизации не удачный, выходим
                        if (frameAuthorization.AuthorizationResult == false)
                        {
                            return; 
                        }

                        //Запись атрибутов принятого задания в ПЛК
                        GTIN_TAG.Write(SelectedWorkAssignment.gtin);
                        TASK_ID_TAG.Write(SelectedWorkAssignment.ID);
                        PRODUCT_NAME_TAG.Write(SelectedWorkAssignment.productName);
                        LOT_NO_TAG.Write(SelectedWorkAssignment.lotNo);
                        NUM_PACKS_IN_BOX_TAG.Write(SelectedWorkAssignment.numРacksInBox);
                        NUM_PACKS_IN_SERIES_TAG.Write(SelectedWorkAssignment.numPacksInSeries);

                        //Запись статуса в ПЛК
                        IN_WORK_TAG.Write(true);

                        //Уведомление подписчиков о принятии задания
                        WorkOrderAcceptanceNotification?.Invoke(SelectedWorkAssignment);

                        string message = $"Задание {TASK_ID_TAG.StatusText} принято в работу";

                        //Запись в базу данных о принятии задания
                        DataBridge.AlarmLogging.AddMessage(message, MessageType.TaskLogging);

                        var msg = new UserMessage(message, MSGTYPE.SUCCES);
                        DataBridge.MSGBOX.Add(msg);

                        return;
                    }
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для завершения задания
        /// </summary>
        public ICommand FinishTaskCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //Проверка - работает ли линия
                    if (DataBridge.Conveyor.LineIsRun == true)
                    {
                        customMessageBox mb = new customMessageBox("Ошибка", "Задание в работе, перед завершением остановите конвейер.");
                        mb.Owner = DataBridge.MainScreen;
                        mb.ShowDialog();

                        return;
                    }


                    //Вызов окна авторизации
                    SortingStantion.UserAdmin.frameAuthorization frameAuthorization = new SortingStantion.UserAdmin.frameAuthorization();
                    frameAuthorization.ShowDialog();

                    //Если результат авторизации не удачный, выходим
                    if (frameAuthorization.AuthorizationResult == false)
                    {
                        return;
                    }

                    Action action = () =>
                    {
                        //Стирание данных в ПЛК
                        GTIN_TAG.Write("");
                        TASK_ID_TAG.Write("");
                        PRODUCT_NAME_TAG.Write("");
                        LOT_NO_TAG.Write("");

                        //Обнуление счетчиков изделий
                        NUM_PACKS_IN_BOX_TAG.Write(0);
                        NUM_PACKS_IN_SERIES_TAG.Write(0);

                        QUANTITY_WORKSPACE.Write(0);
                        QUANTITY_BOXS.Write(0);
                        QUANTITY_WORKSPACE_AUTO_REJECTED.Write(0);
                        QUANTITY_WORKSPACE_MANUAL_REJECTED.Write(0);
                        REPEAT_COUNTER.Write(0);
                        DEFECT_COUNTER.Write(0);


                        //Запись статуса в ПЛК
                        IN_WORK_TAG.Write(false);

                        //Запись в базу данных
                        var message = $"Завершена работа по заданию ID: {SelectedWorkAssignment.ID}";

                        //Запись в базу данных о принятии задания
                        DataBridge.AlarmLogging.AddMessage(message, MessageType.TaskLogging);

                        var msg = new UserMessage(message, MSGTYPE.SUCCES);
                        DataBridge.MSGBOX.Add(msg);

                        //Сброс результата 
                        try
                        {
                            DataBridge.Report.SendReport();

                            //Уведомление подписчиков о завершении задания
                            WorkOrderCompletionNotification?.Invoke(SelectedWorkAssignment);

                            WorkAssignments.Clear();                            
                        }
                        catch (Exception ex)
                        {
                            //Запись в базу данных
                            message = $"Ошибка отправки отчета";
                            msg = new UserMessage(message, DataBridge.myRed);
                            DataBridge.MSGBOX.Add(msg);
                        }
                    };

                    var wcr = new windowClearCollectionRequest(action);




                    return;
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Метод для запуcка http listener
        /// </summary>
        async void StartListener()
        {
            //Получение адреса ПК, запросы от которого необходимо
            //прослушивать для получения задания
            var prefixe = DataBridge.SettingsFile.GetValue("SrvL3Url") + "/";

            //Регистрация url
            NetAclChecker.AddAddress("http://192.168.3.97:7080/jobs/");
            //NetAclChecker.AddAddress("http://localhost:7080/jobs/");

            //Инициализация экземпляра  listener
            HttpListener listener = new HttpListener();

            //Установка адресов  listener
            listener.Prefixes.Add("http://192.168.3.97:7080/jobs/");
            //listener.Prefixes.Add("http://localhost:7080/jobs/");

            //Запуск слушателя
            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                //Запись в лог
                Logger.AddExeption("WorkAssignmentEngine.cs", ex);

                //Выход из функции
                return;
            }


            //Цикл для бесконечной прослушки listener
            while (true)
            {
                //метод GetContext блокирует текущий поток, ожидая получение запроса
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                // получаем объект ответа
                HttpListenerResponse response = context.Response;

                //Читаем  данные из ответа
                string data = string.Empty;
                using (var reader = new StreamReader(request.InputStream, System.Text.Encoding.UTF8))
                {
                    data = reader.ReadToEnd();
                }

                //Если данные не пустая строка - производим десериализацию
                var workAssignment = JsonConvert.DeserializeObject<WorkAssignment>(data);

                // создаем ответ в виде кода html
                string responseStr = "201";

                //Проверка задания и перенос задачи в текущую задачу
                var result = CheckTask(workAssignment);

                //Объявление локального делегата
                Action action = null;

                //В случае, если проверка прошла 
                //успешно
                if (result == true)
                {
                    //Инициализация делегата
                    action = () =>
                    {
                        var msg = new UserMessage($"На ПК поступило задание {workAssignment.ID}. Задание может быть принято в работу", DataBridge.myGreen);
                        DataBridge.MSGBOX.Add(msg);
                    };

                    //Выводим в потоке UI сообщение
                    DataBridge.MainScreen.Dispatcher.Invoke(action);

                    //Перенос свойств в задание которое может быть
                    //принято в работу
                    if (WorkAssignments.Count == 0)
                    {
                        //Добавление принятого задания в коллекцию заданий
                        WorkAssignments.Add(workAssignment);

                        //Уведомление подписчиков о получении нового задания от L3
                        NewWorkOrderHasArrivedNotification?.Invoke(workAssignment);
                    }
                    else
                    {
                        WorkAssignments[0] = workAssignment;
                    }
                    
                }

                //В случае, если в процедуре принятия задания
                //произошла ошибка
                if (result == false)
                {
                    //формирование ответа                   
                    responseStr = "404 Bad Request";
                }

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);

                // получаем поток ответа и пишем в него ответ
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                // закрываем поток
                output.Close();
            }

            // останавливаем прослушивание подключений
            listener.Stop();
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