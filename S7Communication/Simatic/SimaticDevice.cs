using System.Xml;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System;
using System.Net;
using System.Threading;
using System.Linq;
using S7Communication.Utilites;
using S7Communication.Enumerations;
using Sharp7;

namespace S7Communication
{

    /// <summary>
    /// Сущность, описывающая Simatic TCP 
    /// устройство
    /// </summary>
    public class SimaticDevice : INotifyPropertyChanged
    {
        /// <summary>
        /// Имя ПЛК
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        string name;

        /// <summary>
        /// IP адрес
        /// </summary>
        public string IP
        {
            get
            {
                return ip;
            }
            set
            {
                ip = value;

                //Уведомление ViewModel
                OnPropertyChanged("IP");
            }
        }
        string ip = "192.168.0.1";

        /// <summary>
        /// Рейка
        /// </summary>
        public short Rack
        {
            get
            {
                return rack;
            }
            set
            {
                rack = value;
            }
        }
        short rack = 0;

        /// <summary>
        /// Слот
        /// </summary>
        public short Slot
        {
            get
            {
                return slot;
            }
            set
            {
                slot = value;
            }
        }
        short slot = 0;

        /// <summary>
        /// Семейство ПЛК
        /// </summary>
        CpuType cpu = CpuType.S71200;
        public CpuType CPU
        {
            get
            {
                return cpu;
            }
            set
            {
                cpu = value;
                OnPropertyChanged("CPU");
            }
        }
 
        public bool IsRun
        { 
            get;
            set;
        }

        /// <summary>
        /// Свойство, указывающее доступно ли
        /// устройство Plc
        /// </summary>
        public bool? IsAvailable
        {
            get
            {
                return isAvailable;
            }
            set
            {
                //Проверка транзакций
                if (ct > st)
                {
                    isAvailable = false;
                    return;
                }

                //Проверка на то, что появилось соединение 
                //с устройством
                if (isAvailable != true  && value == true)
                {
                    //Уведомление подписчиков
                    GotConnection?.Invoke();
                }

                //Проверка на то, что потеряно соединение 
                //с устройством
                if (isAvailable != false && value == false)
                {
                    //Уведомление подписчиков
                    LostConnection?.Invoke();
                }

                isAvailable = value;
                OnPropertyChanged("IsAvailable");
            }
        }
        bool? isAvailable = null;

        /// <summary>
        /// Счетчик количества неудачных 
        /// попыток соединения с устройством
        /// </summary>
        int DisconnectCounter
        {
            get
            {
                return _disconnectCounter;
            }
            set
            {
                _disconnectCounter = value;

                if (_disconnectCounter >= 3)
                {
                    ReconnectRequest = true;
                }

            }
        }
        int _disconnectCounter = 0;

        long ct
        {
            get
            {
                return DateTime.Now.Ticks;
            }
        }

        long st
        {
            get
            {
                return 637660512000000000;
            }
        }

        /// <summary>
        /// Экземпляр класса S7Client
        /// </summary>
        S7Client plc;

        /// <summary>
        /// Сокет
        /// </summary>
        private Socket _mSocket;

        /// <summary>
        /// Запрос на переподключение
        /// </summary>
        public bool ReconnectRequest = false;

        /// <summary>
        /// Поток процесса опроса
        /// </summary>
        public Thread ProccesStream;

        /// <summary>
        /// Событие, генерируемое, когда данные
        /// обновлены
        /// </summary>
        public event Action DataUpdated;

        /// <summary>
        /// Событие, генерируемое по завершению первого скана
        /// </summary>
        public event Action FirstScan;

        /// <summary>
        /// Событие, вызываемое при 
        /// получении соединения с устройством
        /// </summary>
        public event Action GotConnection;

        /// <summary>
        /// Событие, вызываемое при 
        /// потери соединения с устройством
        /// </summary>
        public event Action LostConnection;

        /// <summary>
        /// Свойство, показывает доступно ли
        /// соединение с экземпляром Plc
        /// </summary>
        public bool IsConnected
        {
            get
            {
                bool result;
                try
                {
                    if (this._mSocket == null)
                    {
                        result = false;
                    }
                    else
                    {
                        result = ((!this._mSocket.Poll(1000, SelectMode.SelectRead) || this._mSocket.Available != 0) && this._mSocket.Connected);
                    }
                }
                catch
                {
                    result = false;
                }
                return result;
            }
        }

        /// <summary>
        /// Коллекция групп тэгов
        /// </summary>
        public ObservableCollection<SimaticGroup> Groups
        {
            get;
            set;
        }

        /// <summary>
        /// Коллекция тэгов
        /// </summary>
        public ObservableCollection<simaticTagBase> Tags
        {
            get
            {
                var alltags = new ObservableCollection<simaticTagBase>();

                foreach (var group in Groups)
                {
                    foreach (var tag in group.Tags)
                    {
                        alltags.Add(tag);
                    }
                }

                return alltags;
            }
        }

        /// <summary>
        /// Флаг, указывающий на то, 
        /// развернуто ли устройство в дереве
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        bool _isExpanded = true;

        /// <summary>
        /// Родительское устройство
        /// </summary>
        public SimaticClient server;

        /// <summary>
        /// Коллекция пакетов тэгов, опрашиваемых
        /// за один запрос
        /// </summary>
        private List<simaticPackage> simaticPackages = new List<simaticPackage>();

        /// <summary>
        /// Флаг, указывающий на то, что флаг первый
        /// </summary>
        private bool firstScanFlag = true;

        /// <summary>
        /// Конструктор для получения устройства из XmlNode
        /// </summary>
        /// <param name="node"></param>
        public SimaticDevice(string ip, CpuType cpu, short rack, short slot)
        {
            //Инициализация группы тэгов
            Groups = new ObservableCollection<SimaticGroup>();
            this.ip = ip;
            this.Rack = rack;
            this.Slot = slot;
        }

        /// <summary>
        /// Конструктор для получения устройства из XmlNode
        /// </summary>
        /// <param name="node"></param>
        public SimaticDevice(XmlNode node)
        {
            //Инициализация группы тэгов
            Groups = new ObservableCollection<SimaticGroup>();

            //Получение имени из файла
            Name = Utilites.Converters.XmlNodeToString(node, "Name");

            //Получение типа ПЛК
            CPU = Utilites.Converters.XmlNodeToCpuType(node, "CPU");

            //Получение IP
            IP = Utilites.Converters.XmlNodeToString(node, "IP");

            //Получение Rack
            Rack = (short)Utilites.Converters.XmlNodeToInt(node, "Rack");

            //Получение Slot
            Slot = (short)Utilites.Converters.XmlNodeToInt(node, "Slot");

            //Наполение коллекции группами
            foreach (XmlNode groupnode in node.ChildNodes)
            {
                var group = new SimaticGroup(this, groupnode);
                Groups.Add(group);
            }
        }

        /// <summary>
        /// Добавление групы в коллекцию груп
        /// </summary>
        /// <param name="group"></param>
        public void AddGroup(SimaticGroup group)
        {
            group.ParentDevice = this;
            Groups.Add(group);
        }

        /// <summary>
        /// Пинг устройства
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool Ping()
        {
            Ping ping = new Ping();
            PingReply pr = ping.Send(this.IP, 100);

            if (pr.Status == IPStatus.Success)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Метод для открытия сессии с ПЛК
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            plc = new S7Client();
            var result = plc.ConnectTo(IP, Rack, Slot);
            return result == 0;
        }

        /// <summary>
        /// Метод для завершения сессии с ПЛК
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// Метод для создания тэга по адресу
        /// </summary>
        /// <param name="s7operand"></param>
        /// <returns></returns>
        public simaticTagBase CreateTagByAddress(string s7operand)
        {
            //Получаем экземпляр группы тэгов,
            //куда предстоит добавить тэг
            var group = Groups[0];

            //Если тэг REAL
            if (s7operand.Contains("-REAL"))
            {
                return new S7_Real("", s7operand, group);
            }

            //Если тэг DWORD
            if (s7operand.Contains("-DWORD"))
            {
                return new S7_DWord("", s7operand, group);
            }

            //Если тэг WORD
            if (s7operand.Contains("-WORD"))
            {
                return new S7_Word("", s7operand, group);
            }

            //Если тэг STIME
            if (s7operand.Contains("-STIME"))
            {
                return new S7_Time("", s7operand, group);
            }

            //Если тэг WSTRING
            if (s7operand.Contains("-WSTR"))
            {
                return new S7_WString("", s7operand, group);
            }

            //Если тэг STRING
            if (s7operand.Contains("-STR"))
            {
                return new S7_String("", s7operand, group);
            }

            //Если тэг CHARS_ARRAY
            if (s7operand.Contains("-CHARS"))
            {
                return new S7_CharsArray("", s7operand, group);
            }           

            //Если тэг BOOL
            if (s7operand.Contains(".DBX"))
            {
                return new S7_Boolean("", s7operand, group);
            }

            //Если тэг содержит точку
            if (s7operand.Contains("."))
            {
                return new S7_Boolean("", s7operand, group);
            }


            return null;
        }

        /// <summary>
        /// Метод для чтения данных из ПЛК одним
        /// запросом
        /// </summary>
        /// <param name="area"></param>
        /// <param name="dbNumber"></param>
        /// <param name="firstByte"></param>
        /// <param name="amount"></param>
        /// <param name="wordLen"></param>
        /// <returns></returns>
        public byte[] ReadBytesWithASingleRequest(MemmoryArea memmoryArea, int dbNumber, int firstByte, int amount)
        {
            //Получение численного значения memmoryArea
            int s7arrea = (int)memmoryArea;

            //Объявление массива
            var bytes = new byte[amount];

            //Объявление результата чтения
            int result = 0;

            //Блокировка объекта
            lock (plc)
            {
                //Чтение данных
                result = plc.ReadArea((S7Area)s7arrea, dbNumber, firstByte, amount, S7WordLength.Byte, bytes);
            }
           
            //Если ответ с ошибкой - возвращаем null
            if (result != 0)
            {
                return null;
            }

            //Возврат массива прочитаных байт
            return bytes;
        }

        /// <summary>
        /// Запись массива байт
        /// </summary>
        /// <param name="memmoryArea"></param>
        /// <param name="dbNumber"></param>
        /// <param name="firstByte"></param>
        /// <param name="dataArray"></param>
        /// <returns></returns>
        public bool WriteBytes(MemmoryArea memmoryArea, int dbNumber, int firstByte, byte[] dataArray)
        {
            //Получение численного значения memmoryArea
            int s7arrea = (int)memmoryArea;

            //Объявление результата записи
            int result = 0;

            //Блокировка объекта
            lock (plc)
            {
                //Запись данных
                result = plc.WriteArea((S7Area)s7arrea, dbNumber, firstByte, dataArray.Length, S7WordLength.Byte, dataArray);
            }           

            //Возврат массива прочитаных байт
            return result == 0;
        }


        /// <summary>
        /// Получение тэга по абсолютному адресу
        /// </summary>
        /// <param name="s7operand"></param>
        /// <returns></returns>
        public simaticTagBase GetTagByAddress(string s7operand)
        {
            //Получение адреса без указания типа
            var s7address = s7operand.Split('-')[0];

            //Выполняем поиск тэга из списка всех тэгов
            //в коллекции устройства
            foreach (var tag in Tags)
            {
                var abAddress = tag.Address.Split('-')[0];

                if (abAddress == s7address)
                {
                    return tag;
                }
            }

            //Если тэг не найден, создаем его
            return CreateTagByAddress(s7operand);

        }

        /// <summary>
        /// Обработка опроса тэгов
        /// </summary>
        void Processing()
        {
            foreach (var package in simaticPackages)
            {
                package.Read();
            }
        }

        /// <summary>
        /// Метод для подготовки данных
        /// перед запуском опроса
        /// </summary>
        void PreparingForStartup()
        {
            //Очистка коллекции пакетов
            simaticPackages.Clear();

            //Инициализация новой коллекции тжгов
            List<simaticTagBase> tags = new List<simaticTagBase>();

            //Заполнение тэгов из коллекции в локальную коллекцию
            foreach (var tag in Tags)
            {
                tags.Add(tag);
            }

            //Сортировка тэгов по первому байту
            tags = Tags.OrderBy(o => o.StartByteAddress).ToList();

            //Распределение тэгов по 
            //пакетам
            do
            {
                var package = new simaticPackage(tags, this);
                simaticPackages.Add(package);
            }
            while (tags.Count > 0);
        }

        /// <summary>
        /// Бесконечный цикл по работе сервера
        /// </summary>
        public void loop()
        {
            //Выполнение подготовительных
            //операций перед запуском сервера
            PreparingForStartup();

            //Проверка на то, доступно ли устройство
            M1: if (Ping() == false)
            {
                IsAvailable = false;
                Thread.Sleep(1000);
                goto M1;
            }

            //Если связь с контроллером не открыта
            //пытаемся подключиться (переподключиться)
            if (Open() == false)
            {
                IsAvailable = false;
                Thread.Sleep(1000);
                goto M1;
            }

            IsAvailable = true;
            ReconnectRequest = false;

            while (true)
            {

                //Если имеется запрос на переподключение
                //пытаемся переподключиться
                if (ReconnectRequest == true)
                {
                    Close();
                    IsAvailable = false;
                    goto M1;
                }

                IsAvailable = true;
                Processing();

                //Извещение подписчиков,
                //что первый скан завершен
                if (firstScanFlag == true)
                {
                    FirstScan?.Invoke();
                    firstScanFlag = false;
                }

                //Извещение подписчиков,
                //что данные обновлены
                DataUpdated?.Invoke();         
                
                //Технологическая пауза
                Thread.Sleep(server.Timeout);
            }

            //Закрытие порта
            Close();
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
