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

        /// <summary>
        /// Свойство, указывающее доступно ли
        /// устройство Plc
        /// </summary>
        bool isAvailable = false;
        public bool IsAvailable
        {
            get
            {
                return isAvailable;
            }
            set
            {
                //Проверка на то, что появилось соединение 
                //с устройством
                if (IsAvailable == false && value == true)
                {
                    //Уведомление подписчиков
                    GotConnection?.Invoke();
                }

                //Проверка на то, что потеряно соединение 
                //с устройством
                if (IsAvailable == true && value == false)
                {
                    //Уведомление подписчиков
                    LostConnection?.Invoke();
                }

                isAvailable = value;
                OnPropertyChanged("IsAvailable");
            }
        }

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
        /// Свойство для MVVM
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");

                //Костыль
                server.FindeSelectedElement();
            }
        }
        bool _isSelected;

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
        public SimaticServer server;

        /// <summary>
        /// Коллекция пакетов тэгов, опрашиваемых
        /// за один запрос
        /// </summary>
        private List<simaticPackage> simaticPackages = new List<simaticPackage>();

        /// <summary>
        /// Конструктор для получения устройства из XmlNode
        /// </summary>
        /// <param name="node"></param>
        public SimaticDevice(string ip, CpuType cpu, short rack, short slot)
        {
            //Инициализация группы тэгов
            Groups = new ObservableCollection<SimaticGroup>();
            Rack = rack;
            Slot = slot;
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
        public bool PING(string ip)
        {
            Ping ping = new Ping();
            PingReply pr = ping.Send(ip);

            if (pr.Status == IPStatus.Success)
            {
                return true;
            }

            return true;
        }

        /// <summary>
        /// Метод для открытия сессии с ПЛК
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            
            //Инициализация буфера
            byte[] buffer = new byte[256];

            //Попытка создать сокет
            //и подключится к нему
            try
            {
                this._mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 100);
                this._mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 100);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(this.IP), 102);
                this._mSocket.Connect(remoteEP);
                
                //Возврат значения, указывающего
                //на отсутсвие ошибок чтения
                //return true;
            }
            catch (Exception ex)
            {
                return false;
            }

            try
            {
                byte[] array = new byte[] 
                { 
                    3, 0, 0, 22, 17, 224, 0, 0, 0, 46, 0, 193, 2, 1, 0, 194, 2, 3, 0, 192, 1, 9 
                };

                CpuType cPU = this.CPU;
                
                
                if (cPU <= CpuType.S7300)
                {
                    if (cPU == CpuType.S7200)
                    {
                        array[11] = 193;
                        array[12] = 2;
                        array[13] = 16;
                        array[14] = 0;
                        array[15] = 194;
                        array[16] = 2;
                        array[17] = 16;
                        array[18] = 0;
                        goto IL_241;
                    }
                    if (cPU != CpuType.S7300)
                    {
                        goto IL_23A;
                    }
                }
                else
                {
                    if (cPU == CpuType.S7400)
                    {
                        array[11] = 193;
                        array[12] = 2;
                        array[13] = 1;
                        array[14] = 0;
                        array[15] = 194;
                        array[16] = 2;
                        array[17] = 3;
                        array[18] = (byte)(this.Rack * 2 * 16 + this.Slot);
                        goto IL_241;
                    }
                    if (cPU != CpuType.S71200)
                    {
                        if (cPU != CpuType.S71500)
                        {
                            goto IL_23A;
                        }
                        array[11] = 193;
                        array[12] = 2;
                        array[13] = 16;
                        array[14] = 2;
                        array[15] = 194;
                        array[16] = 2;
                        array[17] = 3;
                        array[18] = (byte)(this.Rack * 2 * 16 + this.Slot);
                        goto IL_241;
                    }
                }
                array[11] = 193;
                array[12] = 2;
                array[13] = 1;
                array[14] = 0;
                array[15] = 194;
                array[16] = 2;
                array[17] = 3;
                array[18] = (byte)(this.Rack * 2 * 16 + this.Slot);
                goto IL_241;
                IL_23A:
                ErrorCode result = ErrorCode.WrongCPU_Type;
                return false;
                IL_241:

                //Отправка телеграммы
                this._mSocket.Send(array, 22, SocketFlags.None);


                if (this._mSocket.Receive(buffer, 22, SocketFlags.None) != 22)
                {
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                }

                byte[] buffer2 = new byte[]
                {
                    3, 0, 0, 25, 2, 240, 128, 50, 1, 0, 0, 255, 255, 0, 8, 0, 0, 240, 0, 0, 3, 0, 3, 1, 0
                };

                this._mSocket.Send(buffer2, 25, SocketFlags.None);
                if (this._mSocket.Receive(buffer, 27, SocketFlags.None) != 27)
                {
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                }
            }
            catch (Exception ex2)
            {
                
                return false;
            }
            return true;
        }


        /// <summary>
        /// Метод для завершения сессии с ПЛК
        /// </summary>
        public void Close()
        {
            if (this._mSocket != null && this._mSocket.Connected)
            {
                this._mSocket.Shutdown(SocketShutdown.Both);
                this._mSocket.Close();
            }
        }

        /// <summary>
        /// Формирование заголовка
        /// для чтения данных
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private ByteArray ReadHeaderPackage(int amount = 1)
        {
            ByteArray byteArray = new ByteArray(19);
            ByteArray arg_13_0 = byteArray;
            byte[] expr_0F = new byte[3];
            expr_0F[0] = 3;
            arg_13_0.Add(expr_0F);
            byteArray.Add((byte)(19 + 12 * amount));
            byteArray.Add(new byte[]
            {
                2, 240, 128, 50, 1, 0, 0, 0, 0
            });

            byteArray.Add(Word.ToByteArray((ushort)(2 + amount * 12)));
            byteArray.Add(new byte[]
            {
                0,
                0,
                4
            });
            byteArray.Add((byte)amount);
            return byteArray;
        }

        /// <summary>
        /// Формирование запроса для чтения данных
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private ByteArray CreateReadDataRequestPackage(MemmoryArea dataType, int db, int startByteAdr, int count = 1)
        {
            ByteArray byteArray = new ByteArray(12);
            byteArray.Add(new byte[]
            {
                18,
                10,
                16
            });
            if (dataType == MemmoryArea.Counter || dataType == MemmoryArea.Timer)
            {
                byteArray.Add((byte)dataType);
            }
            else
            {
                byteArray.Add(2);
            }
            byteArray.Add(Word.ToByteArray((ushort)count));
            byteArray.Add(Word.ToByteArray((ushort)db));
            byteArray.Add((byte)dataType);
            int num = (int)((long)(startByteAdr * 8) / 65535L);
            byteArray.Add((byte)num);
            if (dataType == MemmoryArea.Counter || dataType == MemmoryArea.Timer)
            {
                byteArray.Add(Word.ToByteArray((ushort)startByteAdr));
            }
            else
            {
                byteArray.Add(Word.ToByteArray((ushort)(startByteAdr * 8)));
            }
            return byteArray;
        }

        /// <summary>
        /// Метод для чтения байт за один запрос
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytesWithASingleRequest(MemmoryArea dataType, int db, int startByteAdr, int count)
        {
            byte[] array = new byte[count];
            byte[] result;
            try
            {
                ByteArray byteArray = new ByteArray(31);
                byteArray.Add(this.ReadHeaderPackage(1));
                byteArray.Add(this.CreateReadDataRequestPackage(dataType, db, startByteAdr, count));
                this._mSocket.Send(byteArray.array, byteArray.array.Length, SocketFlags.None);
                byte[] array2 = new byte[512];
                this._mSocket.Receive(array2, 512, SocketFlags.None);
                
                //Неверное количество байт
                if (array2[21] != 255)
                {
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                }

                //Перенос байт в ответ
                for (int i = 0; i < count; i++)
                {
                    array[i] = array2[i + 25];
                }

                //Возврат ответа
                result = array;
            }
            catch (SocketException ex)
            {
                //Переподключение
                ReconnectRequest = true;
                result = null;
            }
            catch (Exception ex2)
            {
                ReconnectRequest = true;
                result = null;
            }
            return result;
        }


        // <summary>
        /// Метод для записи значения переменной
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteBytes(MemmoryArea dataType, int db, int startByteAdr, byte[] value)
        {
            //Проверка на нулевой
            //указатель
            if (_mSocket == null)
            {
                return false;
            }

            byte[] array = new byte[513];

            try
            {
                int num = value.Length;
                int num2 = 35 + value.Length;
                ByteArray byteArray = new ByteArray(num2);
                ByteArray arg_2C_0 = byteArray;
                byte[] expr_28 = new byte[3];
                expr_28[0] = 3;
                arg_2C_0.Add(expr_28);
                byteArray.Add((byte)num2);
                
                byteArray.Add(new byte[]
                {
                    2, 240, 128, 50, 1, 0, 0
                });

                byteArray.Add(Word.ToByteArray((ushort)(num - 1)));

                byteArray.Add(new byte[]
                {
                    0, 14
                });

                byteArray.Add(Word.ToByteArray((ushort)(num + 4)));

                byteArray.Add(new byte[]
                {
                    5, 1, 18, 10, 16, 2
                });

                byteArray.Add(Word.ToByteArray((ushort)num));
                byteArray.Add(Word.ToByteArray((ushort)db));
                byteArray.Add((byte)dataType);
                int num3 = (int)((long)(startByteAdr * 8) / 65535L);
                byteArray.Add((byte)num3);
                byteArray.Add(Word.ToByteArray((ushort)(startByteAdr * 8)));
                byteArray.Add(new byte[]
                {
                    0, 4
                });

                byteArray.Add(Word.ToByteArray((ushort)(num * 8)));
                byteArray.Add(value);

                this._mSocket.Send(byteArray.array, byteArray.array.Length, SocketFlags.None);
                this._mSocket.Receive(array, 512, SocketFlags.None);
                if (array[21] != 255)
                {
                    throw new Exception();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        ///// <summary>
        ///// Проверка соединения
        ///// </summary>
        //public void TestConnection()
        //{
        //    string report = string.Empty;

        //    if (plc.IsAvailable)
        //    {
        //        report += string.Format("Серверу доступно устройство с IP={0}\n", plc.IP);

        //        ErrorCode code = plc.Open();
        //        plc.Close();

        //        if (code != ErrorCode.NoError)
        //        {
        //            report += string.Format(", однако при установке соединения с ПЛК возникает ошибка. ErrorCode={1};", plc.IP, code.ToString());
        //            MessageBox.Show(report, "Информация об проведенном тесте соединения", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        }
        //        else
        //        {
        //            report = string.Format("Тест соединения с ПЛК IP={0} успешно завершен. Соединение с ПЛК установлено!", plc.IP);
        //            MessageBox.Show(report, "Информация об проведенном тесте соединения", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }

        //    }
        //    else
        //    {
        //        MessageBox.Show(string.Format("Устройство с IP={0} недоступно для сервера (не пингуется)", plc.IP), "Информация об проведенном тесте соединения", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //}

        /// <summary>
        /// Метод для получения всех вложенных тэгов
        /// </summary>
        /// <returns></returns>
        public simaticTagBase [] GetTags()
        {
            List<simaticTagBase> tags = new List<simaticTagBase>();

            foreach (var group in Groups)
            {
                foreach (var tag in group.Tags)
                {
                    tags.Add(tag);
                }
            }

            return tags.ToArray();

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
                return new S7REAL("", s7operand, group);
            }

            //Если тэг DWORD
            if (s7operand.Contains("-DWORD"))
            {
                return new S7DWORD("", s7operand, group);
            }

            //Если тэг WORD
            if (s7operand.Contains("-WORD"))
            {
                return new S7WORD("", s7operand, group);
            }

            //Если тэг STIME
            if (s7operand.Contains("-STIME"))
            {
                return new S7TIME("", s7operand, group);
            }

            //Если тэг WSTRING
            if (s7operand.Contains("-WSTR"))
            {
                return new S7_WSTRING("", s7operand, group);
            }

            //Если тэг STRING
            if (s7operand.Contains("-STR"))
            {
                return new S7_STRING("", s7operand, group);
            }

            //Если тэг BOOL
            if (s7operand.Contains(".DBX"))
            {
                return new S7BOOL("", s7operand, group);
            }

            return null;
        }


        /// <summary>
        /// Получение тэга по абсолютному адресу
        /// </summary>
        /// <param name="s7operand"></param>
        /// <returns></returns>
        public simaticTagBase GetTagByAddress(string s7operand)
        {
            
            //Выполняем поиск тэга из списка всех тэгов
            //в коллекции устройства
            foreach (var tag in Tags)
            {
                if (tag.Address == s7operand)
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
            //Сортировка тэгов по первому байту
            tags = Tags.OrderBy(o => o.StartByteAddress).ToList();

            //Распределение тэгов по 
            //коллекции
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

            //Если связь с контроллером не открыта
            //пытаемся подключиться (переподключиться)
            M1: if (Open() == false)
            {
                IsAvailable = false;
                Thread.Sleep(1000);
                goto M1;
            }

            IsAvailable = true;
            ReconnectRequest = false;

            //Prepare();

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
                
                Thread.Sleep(server.Timeout);

                //Извещение подписчиков,
                //что данные обновлены
                DataUpdated?.Invoke();
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
