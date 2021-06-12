using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;

namespace SortingStantion.Models
{
    /// <summary>
    /// Перечисление типов аварий
    /// </summary>
    public enum MessageType
    { 
        Alarm = 0, 
        Info = 1, 
        Event = 2,
        ChangeUser = 3
    }

    /// <summary>
    /// Класс, реализующий модель архиватора
    /// сообщений в базу данных
    /// </summary>
    public class AlarmLogging
    {
        /// <summary>
        /// Путь до книги Excel
        /// </summary>
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                //Если данные изменились
                fileName = value;
                OnPropertyChange("FileName");
            }
        }
        string fileName = "hmi.db";


        /// <summary>
        /// Подключение к источнику данных
        /// </summary>
        SQLiteConnection connection;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="ConnectionString">Строка соединения</param>
        public AlarmLogging()
        {
            //При отсутствии папки AlarmLogging - создаем её
            if (Directory.Exists("AlarmLogging") == false)
            {
                Directory.CreateDirectory("AlarmLogging");
            }
        }

        /// <summary>
        /// Диструктор
        /// </summary>
        public void Dispose()
        {
            connection.Close();
        }

        /// <summary>
        /// Метод для подключение к базе данных
        /// </summary>
        /// <returns>Результат выполнения функции (TRUE - выыполнено, FALSE - не выполнено)</returns>
        public bool Connect()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                //Запись в лог
                Logger.AddExeption("AlarmLogger.cs", ex);
            }

            return false;
        }

        /// <summary>
        /// Метод для отключения от базы данных
        /// </summary>
        /// <returns>Результат выполнения функции (TRUE - выыполнено, FALSE - не выполнено)</returns>
        public bool Disconnect()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                //Запись в лог
                Logger.AddExeption("SQLite.cs functon=Disconnect", ex);
            }

            return false;
        }

       
        /// <summary>
        /// Метод для получени из базы данных
        /// Истории аварий
        /// </summary>
        /// <param name="DateTimeStart"></param>
        /// <param name="DateTimeStop"></param>
        /// <returns></returns>
        //public mbDiscreteAlarm[] GetAlarmList(DateTime DateTimeStart, DateTime DateTimeStop)
        //{
        //    //Форматируем дату-время для обращения к БД
        //    string stStrt = DateTimeStart.ToString("yyyy-MM-dd HH:mm");
        //    string stStp = DateTimeStop.ToString("yyyy-MM-dd HH:mm");

        //    //Список архивных аварий
        //    var list = new List<mbDiscreteAlarm>();

        //    //Формируем запрос к базе данных с сортировкой
        //    string Query = $"SELECT * FROM AlarmHistory WHERE ([dt] BETWEEN '{stStrt}' And '{stStp}') ORDER BY dt DESC;";
        //    //string Query = $"SELECT * FROM AlarmHistory";

        //    //Получение данных из базы данных
        //    var items = Read(Query);

        //    //Наполнение коллекции архивных аварий
        //    foreach (var item in items)
        //    {
        //        //Разделение данных по разделителю
        //        var components = item.Split('\t');

        //        //Построение объекта DISCRET_ALARM
        //        //из принятой строки
        //        var alarmHistoryItem = new mbDiscreteAlarm();

        //        //Получение даты-времени
        //        alarmHistoryItem.DateTime = DateTime.Parse(components[2]);

        //        //Передача данных экземпляру аварии
        //        alarmHistoryItem.ID = components[1];
        //        alarmHistoryItem.Message = components[4];
        //        alarmHistoryItem.UserName = components[5];
        //        alarmHistoryItem.MessageType = GetMessageType(components[3]);

        //        //Формирование списка аварий
        //        //(добавление аварии в начало коллекции)
        //        list.Insert(0, alarmHistoryItem);
        //    }

        //    //Возврат данных в виде массива
        //    return list.ToArray();
        //}

        /// <summary>
        /// Получение типа сообщения из БД
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        MessageType GetMessageType(string code)
        {
            int id = 0;

            bool res = int.TryParse(code, out id);

            switch (id)
            {
                case 1: return MessageType.Alarm;
                case 3: return MessageType.Event;
                default: return MessageType.Event;
            }

        }

        /// <summary>
        /// Метод для добавления нового 
        /// сообщения
        /// </summary>
        /// <param name="alarm"></param>
        /// <returns></returns>
        public bool AddMessage(string message, MessageType type)
        {
            //Получаем текущее время
            var dt = DateTime.Now;

            //Получаем имя базы данных,
            //в которую необходимо внести запись
            var database = $@"AlarmLogging\AlarmsHistory_{dt.Year}_{dt.Year + 1}";

            //При необходимости создаем базу данных
            if (File.Exists(database) == false)
            {
                CreatDataBase(database);
            }
            
            //Получаем сообщение
            var Message = message;

            var userName = "не авторизован";

            if (DataBridge.MainAccesLevelModel.CurrentUser != null)
            {
                userName = DataBridge.MainAccesLevelModel.CurrentUser.Name;
            }
            
            //Формирование запроса к базе данных
            string query = $"INSERT INTO AlarmHistory ([DateTime], " +
                           $"MessageType, " +
                           $"Message, " +
                           $"UserName) " +
                           $"VALUES ('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'," +
                           $"{(int)type},  " +
                           $"'{Message}', " +
                           $"'{userName}');";

            //Выполнение запроса
            return Execute(query);
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="QueryString">Запрос</param>
        /// <returns></returns>
        public bool Execute(string QueryString)
        {
            //Если нет подключения к базе данных,
            //подключаемся к ней
            
            try
            {
                if (connection == null)
                {
                    GetConnection();
                }

                var command = new SQLiteCommand(QueryString, connection);
                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                //Запись в лог
                Logger.AddExeption("SQLite.cs functon=Execute", ex);
                return false;
            }

        }

        /// <summary>
        /// Метод для чтения данных из базы данных
        /// </summary>
        /// <param name="QueryString">Запрос</param>
        /// <returns></returns>
        string[] Read(string QueryString)
        {
            //Данные из запроса
            List<string> datalist = new List<string>();

            //Если нет подключения к базе данных,
            //подключаемся к ней
            if (connection.State != System.Data.ConnectionState.Open)
            {
                Connect();
            }

            try
            {
                var command = new SQLiteCommand(QueryString, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                int count = 0;

                while (reader.Read())
                {
                    var line = string.Empty;

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        line += $"{reader[i]}\t";
                    }

                    //Добавление строки к списку
                    datalist.Add(line);

                    //Инкрементация счетчика
                    count++;
                }

                reader.Close();
            }
            catch (Exception ex)
            {

            }

            return datalist.ToArray();
        }

        /// <summary>
        /// Метод для создания новой базы данных
        /// </summary>
        /// <param name="filename"></param>
        void CreatDataBase(string filename)
        {
            //Если файл не существует, создаем его
            if (File.Exists(filename) == false)
            {
                try
                {
                    //Создаем файл базы данных
                    SQLiteConnection.CreateFile(filename);

                    //Создаем подключение к базе данных
                    GetConnection(filename);

                    //Создаем таблицу в базе данных
                    CreateTable();
                }
                catch (Exception ex)
                {
                    //Запись в лог
                    Logger.AddExeption("SQLite.cs functon=Execute", ex);
                }
            }
        }

        /// <summary>
        /// Метод для создания таблицы
        /// </summary>
        /// <param name="tableName"></param>
        void CreateTable()
        {
            //Запрос на создание таблицы
            string request = $@"CREATE TABLE 'AlarmHistory'(
                            'ID'    INTEGER,
                            'DateTime'  TEXT,
                            'MessageType'   INTEGER,
                            'Message'   TEXT,
                            'UserName'  TEXT,
                            PRIMARY KEY('ID' AUTOINCREMENT));";



            //Выполнение запроса
            var result = Execute(request);
        }

        /// <summary>
        /// Метод для получения подключения к базе данных
        /// </summary>
        /// <returns></returns>
        SQLiteConnection GetConnection(string filename)
        {
            //Создаем подключение к базе данных
            var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filename};";
            connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Метод для получения подключения к базе данных
        /// </summary>
        /// <returns></returns>
        SQLiteConnection GetConnection()
        {
            //Получаем текущее время
            var dt = DateTime.Now;

            //Получаем имя базы данных,
            //в которую необходимо внести запись
            var database = $@"AlarmLogging\AlarmsHistory_{dt.Year}_{dt.Year + 1}";

            //Создаем подключение к базе данных
            var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={database};";
            connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }


        #region РЕАДИЗАЦИЯ ИНТЕРФЕЙСА INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChange(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        #endregion
    }
}
