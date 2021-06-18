using Newtonsoft.Json;
using S7Communication;
using SortingStantion.TechnologicalObjects;
using SortingStantion.Utilites;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SortingStantion.Models
{
    /// <summary>
    /// Объект, осуществляющий управлением
    /// заданиями
    /// </summary>
    public class WorkAssignmentEngine
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
        S7BOOL IN_WORK_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Уникальный идентификатор задания.
        /// </summary>
        public S7_STRING TASK_ID_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Номер GTIN. (14 символов)
        /// </summary>
        public S7_STRING GTIN_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Наименование продукта (UTF-8)
        /// </summary>
        public S7_STRING PRODUCT_NAME_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Номер производственной серии (до 20 символов) 
        /// </summary>
        public S7_STRING LOT_NO_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Кол-во продуктов в коробе. 
        /// </summary>
        public S7DWORD NUM_PACKS_IN_BOX_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Ожидаемое количество продуктов в серии 
        /// (определяется по заданию на производство серии) 
        /// </summary>
        public S7DWORD NUM_PACKS_IN_SERIES_TAG
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
            IN_WORK_TAG = new S7BOOL("", "DB1.DBX182.0", group);
            TASK_ID_TAG = (S7_STRING)device.GetTagByAddress("DB1.DBD184-STR40");
            GTIN_TAG = (S7_STRING)device.GetTagByAddress("DB1.DBD226-STR40");
            LOT_NO_TAG = (S7_STRING)device.GetTagByAddress("DB1.DBD352-STR40");
            PRODUCT_NAME_TAG = (S7_STRING)device.GetTagByAddress("DB1.DBD268-STR82");
            NUM_PACKS_IN_BOX_TAG = (S7DWORD)device.GetTagByAddress("DB1.DBD396-DWORD");
            NUM_PACKS_IN_SERIES_TAG = (S7DWORD)device.GetTagByAddress("DB1.DBD398-DWORD");
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
                //Защита от пустого указателя
                if (IN_WORK_TAG == null)
                {
                    return false;
                }

                //Если тип не bool возвращаем false
                if (IN_WORK_TAG.Status is bool == false)
                {
                    return false;
                }

                //Возвращение значения тэга
                return (bool)IN_WORK_TAG.Status;
            }
            set
            {
                IN_WORK_TAG.Write(value);
            }
        }
               
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
                return WorkAssignments[0];
            }
            set
            {
                WorkAssignments[0] = value;
            }
        }


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
        /// Конструктор класса
        /// </summary>
        public WorkAssignmentEngine()
        {
            //Инициализация коллекции с рабочими заданиями
            WorkAssignments = new ObservableCollection<WorkAssignment>();

            //Инициализация переменных ПЛК
            PlcDataInit();

            StreamReader sr = new StreamReader("json.txt");
            var text = sr.ReadToEnd();
            sr.Close();
            var tprices = JsonConvert.DeserializeObject<WorkAssignment>(text);

            WorkAssignments.Add(tprices);

            //Запуск задачи по прослушиванию
            //http
            Task.Factory.StartNew(() =>
            {
               // StartListener();
            });
        }


        /// <summary>
        /// Метод для запуcка http listener
        /// </summary>
        async void StartListener()
        {
            //Инициализация экземпляра  listener
            HttpListener listener = new HttpListener();

            //Установка адресов  listener
            listener.Prefixes.Add("http://localhost:8888/connection/");
            listener.Start();

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
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    data = reader.ReadToEnd();
                }

                //Если данные не пустая строка - производим десериализацию
                var tprices = JsonConvert.DeserializeObject<WorkAssignment>(data);


                // создаем ответ в виде кода html
                string responseStr = "201";
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

        /// <summary>
        /// Команда - принять задание
        /// </summary>
        public ICommand AcceptTaskCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //Если задание не принято в работу
                    if (InWork == false)
                    {
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

                        //Уведомление подписчиков о принятии задания
                        WorkOrderAcceptanceNotification?.Invoke(SelectedWorkAssignment);

                        //Запись статуса в ПЛК
                        IN_WORK_TAG.Write(true);

                        //Запись в базу данных о принятии задания
                        DataBridge.AlarmLogging.AddMessage($"Задание {TASK_ID_TAG.StatusText} принято в работу", MessageType.Info);

                        return;
                    }
                },
                (obj) => (InWork == false));
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

                    if (InWork == true)
                    {
                        //Вызываем окно авторизации
                        SortingStantion.UserAdmin.frameAuthorization frameAuthorization = new SortingStantion.UserAdmin.frameAuthorization();
                        frameAuthorization.ShowDialog();

                        //Если результат авторизации не удачный, выходим
                        if (frameAuthorization.AuthorizationResult == false)
                        {
                            return;
                        }

                        //Стирание данных в ПЛК
                        GTIN_TAG.Write("");
                        TASK_ID_TAG.Write("");
                        PRODUCT_NAME_TAG.Write("");
                        LOT_NO_TAG.Write("");
                        NUM_PACKS_IN_BOX_TAG.Write(0);

                        NUM_PACKS_IN_SERIES_TAG.Write(0);

                        //Уведомление подписчиков о завершении задания
                        WorkOrderAcceptanceNotification?.Invoke(SelectedWorkAssignment);

                        //Запись в базу данных
                        var message = $"Завершена работа по заданию ID: {SelectedWorkAssignment.ID}";
                        DataBridge.AlarmLogging.AddMessage(message, Models.MessageType.TaskLogging);

                        //Запись статуса в ПЛК
                        IN_WORK_TAG.Write(false);

                        //Запись в базу данных о завершении задания
                        DataBridge.AlarmLogging.AddMessage($"Задание {TASK_ID_TAG.StatusText} завершено", MessageType.Info);

                        return;
                    }
                },
                (obj) => (InWork == true));
            }
        }


    }
}
