using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Xml;

namespace Communication
{
    public class mbDevice : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Локальный сокет
        /// </summary>
        private Socket _mSocket;

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
                OnPropertyChanged("IP");
            }
        }
        string ip = "192.168.0.1";

        /// <summary>
        /// TCP порт
        /// </summary>
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
                OnPropertyChanged("PORT");
            }
        }
        int port = 502;

        /// <summary>
        /// Свойство имени устройства
        /// </summary>
        public string Name
        {
            get
            {
                return $"MODBUS TCP УСТРОЙСТВО: {IP}:{Port}  ID: {ID}";
            }
        }

        /// <summary>
        /// ID Modbus TCP Slave
        /// </summary>
        public ushort ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        ushort id;

        /// <summary>
        /// Свойство для MVVM, указывающее
        /// на то, развернут ли узел
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
        bool _isExpanded = false;

        /// <summary>
        /// Флаг, показывающий доступно ли устройство
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return isAvailable;
            }
            set
            {
                //Уведомление подписчиков об изменении
                //состояния подключения к целевому устройству
                if (isAvailable != value)
                {
                    ChangeConnectionState?.Invoke(value);
                }

                //Фиксация состояния подключения
                //к целевому устройству
                isAvailable = value;
                OnPropertyChanged("IsAvailable");
            }
        }
        bool isAvailable;

        /// <summary>
        /// Свойство, показывает доступно ли
        /// соединение с экземпляром Device
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
        /// Событие, генерируемое при 
        /// изменении статуса подключения
        /// </summary>
        public event Action<bool> ChangeConnectionState;

        /// <summary>
        /// Время, за которое опрашиваются
        /// все регистры устройства
        /// </summary>
        public TimeSpan CyclicTime = new TimeSpan();

        /// <summary>
        /// Запрос на переподключение
        /// </summary>
        public bool ReconnectRequest = false;

        /// <summary>
        /// Количество тэгов в устройстве
        /// </summary>
        public int RegistrsCount
        {
            get;
            set;
        }

        /// <summary>
        /// Коллекция активных аварий
        /// </summary>
        public ObservableCollection<mbDiscreteAlarm> ActiveAlarms
        {
            get
            {
                return active_alarms;
            }
            set
            {
                active_alarms = value;
            }
        }
        ObservableCollection<mbDiscreteAlarm> active_alarms = new ObservableCollection<mbDiscreteAlarm>();


        /// <summary>
        /// Коллекция групп регистров, входящих 
        /// в состав устройства
        /// </summary>
        public ObservableCollection<mbGroup> Groups
        {
            get
            {
                return groups;
            }
            set
            {
                groups = value;
            }
        }
        ObservableCollection<mbGroup> groups = new ObservableCollection<mbGroup>();

        /// <summary>
        /// Событие, возникаемое при
        /// новой аварии
        /// </summary>
        public event Action<mbDiscreteAlarm> NewAlarmNotification;

        /// <summary>
        /// Пинг устройства
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public delegate bool pingDevice(string ip);
        public pingDevice PingDevice;

        /// <summary>
        /// Указатель на родительский сервер
        /// </summary>
        public mbServer Server
        {
            get;
            set;
        }


        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbDevice()
        {
            PingDevice = (ip) => PING(ip);
        }

        /// <summary>
        /// Добавить группу
        /// </summary>
        /// <param name="group"></param>
        public void AddGroup(mbGroup group)
        {
            group.Device = this;
            Groups.Add(group);
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="xmlNode"></param>
        public mbDevice(XmlNode xmlNode)
        {
            //получаем IP
            this.IP = Utilites.GetAttributString(xmlNode, "IP");

            //Получаем TCP порт
            this.Port = Utilites.GetAttributInt(xmlNode, "Port");

            //Получаем Id
            this.ID = (ushort)Utilites.GetAttributInt(xmlNode, "ID");

            //Получаем группы, входящие в состав устройства
            foreach (XmlNode groupNode in xmlNode.ChildNodes)
            {
                //Если узел - это группа - то добавляем группу
                if (groupNode.Name.ToUpper() == "GROUP")
                {
                    this.AddGroup(new mbGroup(groupNode));
                    continue;
                }
            }

            //Метод для пинга
            PingDevice = (ip) => PING(ip);
        }

        delegate int Operation(int x, int y);

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
        /// Флаг, сообщающий, что устройство
        /// в технологической паузе
        /// </summary>
        public bool IsPaused = false;

        /// <summary>
        /// Флаг, что устройство занято
        /// </summary>
        public bool IsBusy = false;

        /// <summary>
        /// Коллекция всех регистров
        /// </summary>
        List<mbRegisterBase> Registers;

        /// <summary>
        /// Метод для подготовки
        /// регистров к опросу
        /// </summary>
        void Prepare()
        {
            //Получаем коллецию всех регистров
            Registers = new List<mbRegisterBase>();
            foreach (var group in Groups)
            {
                Registers.AddRange(group.GetRegistersCollection());
            }

            //Сортируем регистры по адресу
            Registers = Registers.OrderBy(o => o.Address).ToList();

            //Проходим по каждому регистру, и если есть
            //промежуток с соседними, то наполняем пустыми
            //регистрами
            Registers = FillingRegisterArray(Registers);
        }

        /// <summary>
        /// Метод для заполнения коллекции
        /// пустыми регистрами, для того, чтоб 
        /// сократить количество инртераций
        /// </summary>
        List<mbRegisterBase> FillingRegisterArray(List<mbRegisterBase> list)
        {
            List<mbRegisterBase> newarray = new List<mbRegisterBase>();

            var lastregister = list[0];
            newarray.Add(lastregister);

            for (int i = 1; i < list.Count; i++)
            {
                var currentregister = list[i];

                //Разница
                int dif = currentregister.Address - lastregister.GetEndAddress();

                if (dif > 1 && dif < 21)
                {
                    for (int j = 1; j < dif; j++)
                    {
                        var Register = new mbSingleRegister(lastregister.GetEndAddress() + 1, Types.Ushort);
                        Register.ReadRequest = true;
                        lastregister = Register;
                        newarray.Add(Register);
                    }
                }

                lastregister = currentregister;
                newarray.Add(currentregister);
            }

            return newarray;
        }

        /// <summary>
        /// Бесконечный цикл по работе сервера
        /// </summary>
        public void loop()
        {
            //устанавливаем флаг,
            //что сервер запущен
            isrun = true;

            //Если связь с контроллером не открыта
            //пытаемся подключиться (переподключиться)
            M1: if (Open() == false)
                {
                    IsAvailable = false;
                    LostConnectionNotificationRegisters();
                    Thread.Sleep(1000);
                    goto M1;
                }

                IsAvailable = true;
                ReconnectRequest = false;

                Prepare();

                while (isrun)
                {
                    //Если имеется запрос на переподключение
                    //пытаемся переподключиться
                    if (ReconnectRequest == true)
                    {
                        Close();
                        IsAvailable = false;
                        LostConnectionNotificationRegisters();
                        goto M1;
                    }

                    Processing();
                    //Thread.Sleep(ParentServer.Timeout);

                    //Извещение подписчиков,
                    //что данные обновлены
                    DataUpdated?.Invoke();
                }

                //Закрытие порта
                Close();
        }

        /// <summary>
        /// Извещение моделей об утрате
        /// соединения с устройством
        /// </summary>
        void LostConnectionNotificationRegisters()
        {
            var registers = GetRegisters();

            foreach (var register in registers)
            {
                register.IsValid = false;
            }
        }

        /// <summary>
        /// Переменная, отображающая
        /// статус работы сервера
        /// </summary>
        bool isrun;
        
        /// <summary>
        /// Метод для обработки регистров
        /// (чтение - запись)
        /// </summary>
        public void Processing()
        {
            //Проверяем регистры для записи 
            //и записываем их
            //TaskWriteRegisters();

            //Устанавливаем флаг,
            //что устройство не в паузе
            IsPaused = false;

            //Получаем регистры для опроса            
            //var ReqRegisters = GetRegisters();

            //Создаем помошника для чтения регистров
            HelperReading sqd = new HelperReading(Registers.ToArray(), this);

            //Выполнение чтения регистров
            while (sqd.Execute())
            {
                //Thread.Sleep(50);
            }

            //Устанавливаем флаг,
            //что устройство в паузе
            IsPaused = true;
        }

        /// <summary>
        /// Задача по записи регистров
        /// </summary>
        /// <param name="device"></param>
        void TaskWriteRegisters()
        {
            //Проходим по всем группам
            foreach (var group in Groups)
            {
                //Проходим по всем регистрам и записываем те,
                //которые помечаны флагом
                foreach (var register in group.Registers)
                {
                    if (register.WriteRequest == true)
                    {
                        //register.WriteToDevice();
                    }
                }
            }            
        }

        /// <summary>
        /// Метод для предобазования ObservableCollection<RegistrBase>
        /// в RegistrBase[]
        /// </summary>
        /// <param name="registrs"></param>
        /// <returns></returns>
        public static mbRegisterBase[] CollectionToArray(ObservableCollection<mbRegisterBase> registrs)
        {
            mbRegisterBase[] array = new mbRegisterBase[registrs.Count];

            for (int i = 0; i < registrs.Count; i++)
            {
                array[i] = registrs[i];
            }

            return array;
        }

        /// <summary>
        /// Метод для получения регистров c флагом ReadRequest
        /// </summary>
        /// <returns></returns>
        mbRegisterBase[] GetReadingRegisters()
        {
            List<mbRegisterBase> registers = new List<mbRegisterBase>();

            foreach (var register in GetRegisters())
            {
                if (register.ReadRequest == true)
                {
                    registers.Add(register);
                }
            }

            return registers.ToArray();
        }

        /// <summary>
        /// Метод для получения всех регистров в устройстве
        /// </summary>
        /// <returns></returns>
        mbRegisterBase[] GetRegisters()
        {
            List<mbRegisterBase> registers = new List<mbRegisterBase>();

            foreach (var group in Groups)
            {
                foreach (var register in group.Registers)
                {
                    registers.Add(register);
                }
            }

            return registers.ToArray();
        }

        /// <summary>
        /// Метод для открытия сессии с ПЛК
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            try
            {
                this._mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
                this._mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(this.IP), Port);
                this._mSocket.Connect(remoteEP);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Метод для завершения сессии с ПЛК
        /// </summary>
        public void Close()
        {
            this.isrun = false;

            if (this._mSocket != null && this._mSocket.Connected)
            {
                this._mSocket.Shutdown(SocketShutdown.Both);
                this._mSocket.Close();
            }
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
        /// Метод для получения (в случае отсутствия создания) массива регистров
        /// </summary>
        /// <returns></returns>
        public mbRegisterBase[] GetArrayRegisters(int FirstAddress, int Lenght)
        {
            //Коллекция регистров
            List<mbRegisterBase> list = new List<mbRegisterBase>();

            //Вычисление адреса последнего регистра
            var LastAddress = FirstAddress + Lenght;

            //Ищем или добавляем новые регистры
            for (int i = 0; i < Lenght; i++)
            {
                var register = GetRegisterByAddress($"{Port}:{FirstAddress + i}-U");
                list.Add(register);
            }
          
            return list.ToArray();
        }

        /// <summary>
        /// Метод для получения (а в случае отсутсвия создания) регистра
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public mbRegisterBase GetRegisterByAddress(string Address)
        {
            //Получение устройства
            mbDevice device = this;

            //Если устройство не найдено, возвращаем null
            if (device == null)
            {
                return null;
            }

            var claddress = Utilites.ClearAddressStr(Address);

            var reg_id = Convert.ToInt16(claddress);

            //Поиск регистра в  сервере
            foreach (var group in device.Groups)
            {
                foreach (var reg in group.Registers)
                {
                    if (reg.Address == reg_id)
                    {
                        var bitreg = (mbRegisterBase)reg;
                        return bitreg;
                    }
                }
            }


            //Создаем новый регистр, только в том случае, если сервер
            //остановлен и указан ижентификатор типа
            if (ProccesStream != null)
            {
                if (ProccesStream.IsAlive == true)
                {
                    return null;
                }
            }
            

            var address = Address.Split(':')[1];
            var _group = device.FindOrCreateGroupByID("OTHER");

            //ТИП REAL
            if (Address.Contains("-R"))
            {
                var registr = new mbRealRegister(address.Replace("-R", ""));
                _group.AddRegisrt(registr);
                return registr;
            }

            //ТИП STRING
            if (Address.Contains("-STR"))
            {
                //Получаем длину строки
                int lenght = int.Parse(Address.Split('R')[1]);

                //Получаем адрес первого регистра
                int first_register = int.Parse(Utilites.ClearAddressStr(Address));

                var registr = new mbString(first_register, lenght);
                _group.AddRegisrt(registr);
                return registr;
            }

            //ТИП UINT
            if (Address.ToUpper().Contains("-UINT"))
            {
                var registr = new mbDoubleRegister(address.Replace("-UINT", ""));
                _group.AddRegisrt(registr);
                return registr;
            }

            //ТИП SHORT
            if (Address.Contains("-S"))
            {
                var registr = new mbSingleRegister(address.Replace("-S", ""), Types.Short);
                _group.AddRegisrt(registr);
                return registr;
            }

            //ТИП USHORT
            if (Address.Contains("-U"))
            {
                var registr = new mbSingleRegister(address.Replace("-S", ""), Types.Ushort);
                _group.AddRegisrt(registr);
                return registr;
            }

            //ТИП DateTime
            if (Address.Contains("-DT"))
            {
                var registr = new mbDateTime(address.Replace("-DT", ""));
                _group.AddRegisrt(registr);
                return registr;
            }

            return null;
        }


        /// <summary>
        /// Идентификатор транзакции телеграммы
        /// </summary>
        private UInt16 TRANSACTION;


        /// <summary>
        /// Метод для отправки телеграммы в сеть
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        byte[] SendTelegramm(byte[] _data)
        {
            try
            {
                //Проверка на нулевой указатель
                if (this._mSocket == null)
                {
                    return new byte[0];
                }

                //Отправляем телеграмму
                this._mSocket.Send(_data, _data.Length, SocketFlags.None);

                //Читаем ответ
                byte[] array = new byte[512];
                int count = this._mSocket.Receive(array, 512, SocketFlags.None);

                //Помещаем данные в список
                List<byte> bytes = new List<byte>(512);

                for (int i = 0; i < count; i++)
                {
                    bytes.Add(array[i]);
                }

                //Возвращаем результат
                return bytes.ToArray();
            }
            catch (IOException ex)
            {
                //Пытаемся переконектится
                //this.Connect();
            }
            catch (SocketException ex)
            {
                //Пытаемся переконектится
                //this.Connect();
            }
            catch (System.ObjectDisposedException ex)
            {
                ReconnectRequest = true;
            }

            Close();

            //Возвращаем пустой результат
            return new byte[0];

        }

        /// <summary>
        /// ФУНКЦИЯ 0x06 - Запись значения в HOLDING регистр
        /// </summary>
        /// <param name="RegistrAddress"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteHoldingRegister(ushort RegistrAddress, ushort value)
        {
            byte[] data = new byte[15];

            //Идентификатор транзакции
            byte[] transaction = BitConverter.GetBytes(TRANSACTION);
            data[0] = transaction[1];
            data[1] = transaction[0];

            //Идентификатор протокола
            data[2] = 0;
            data[3] = 0;

            //Длина сообщения
            data[4] = 0;
            data[5] = 9;

            //Адрес устройства
            data[6] = (byte)ID;

            //Код функции
            data[7] = 0x10;

            //Адрес первого регистра
            byte[] addr = BitConverter.GetBytes(RegistrAddress);
            data[8] = addr[1];
            data[9] = addr[0];

            //Количество регистров
            data[10] = 0;
            data[11] = 1;

            //Количество байт далее
            data[12] = 2;

            byte[] values = BitConverter.GetBytes(value);
            data[13] = values[1];
            data[14] = values[0];

            //Отправляем данные в TCP канал
            byte[] bts = new byte[0];

            bts = SendTelegramm(data);

            //Если в посылке байтов меньше, тогда возвращаем false
            if (!(bts.Length > 0))
            {
                return false;
            }


            //Сравниваем байты
            bool Addres_OK = false;
            bool Function_OK = false;
            bool Value_OK = false;
            bool IDTransactionOK = false;

            if (bts.Length >= 12)
            {
                //Сравниваем байты
                Addres_OK = (data[8] == bts[8]) && (data[9] == bts[9]);
                Function_OK = (data[6] == bts[6]) && (data[7] == bts[7]);
                Value_OK = (data[10] == bts[10]) && (data[11] == bts[11]);
                IDTransactionOK = (data[0] == bts[0]) && (data[1] == bts[1]);
            }

            //Инкрементируем ID транзакции
            TRANSACTION++;

            //Возвращаем результат
            return Addres_OK && Function_OK && Value_OK && IDTransactionOK;
        }

        /// <summary>
        /// ФУНКЦИЯ 0x03 - Чтение нескольких HOLDING регистров
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <param name="FirstRegistr"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public List<byte> ReadHoldingRegisters(ushort FirstRegistr, ushort Length)
        {
            //Создаем телеграмму
            byte[] out_telegramm = new byte[12];

            //Идентификатор транзакции
            byte[] transaction = BitConverter.GetBytes(TRANSACTION);
            out_telegramm[0] = transaction[1];
            out_telegramm[1] = transaction[0];

            //Идентификатор протокола
            out_telegramm[2] = 0;
            out_telegramm[3] = 0;

            //Длина сообщения
            out_telegramm[4] = 0;
            out_telegramm[5] = 6;

            //Адрес ведомого устройства
            out_telegramm[6] = (Byte)ID;

            //Код функции
            out_telegramm[7] = 0x3;

            //Начальный адрес регистра
            byte[] _adr = BitConverter.GetBytes(FirstRegistr);
            out_telegramm[8] = _adr[1];
            out_telegramm[9] = _adr[0];

            //Кооличество данных для чтения
            byte[] _length = BitConverter.GetBytes(Length);
            out_telegramm[10] = _length[1];
            out_telegramm[11] = _length[0];

            //Отправляем телеграмму, читаем ответ
            byte[] bts = SendTelegramm(out_telegramm);

            //Если принятых данных меньше 11, тогда выходим из функции
            if (bts.Length < 11)
            {
                return new List<byte>();
            }

            //Инкрементируем ID транзакции
            TRANSACTION++;


            //Собираем данные в список для отправки
            List<byte> ints = new List<byte>();

            for (int i = 9; i < bts.Length; i++)
            {
                ints.Add(bts[i]);
            }

            return ints;
        }

        public void Dispose()
        {
            if (this._mSocket != null && this._mSocket.Connected)
            {
                this._mSocket.Shutdown(SocketShutdown.Both);
                this._mSocket.Close();
            }
        }

        /// <summary>
        /// Поиск группы регистров по ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public mbGroup GetGroupByID(string id)
        {
            foreach (var group in Groups)
            {
                if (group.ID == id)
                {
                    return group;
                }
            }

            return null;
        }

        /// <summary>
        /// Метод для поиска, а в случае
        /// её отсутсвия создания группы тэгов
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public mbGroup FindOrCreateGroupByID(string id)
        {
            //Получаем группу регистров
            var group = GetGroupByID(id);

            //Если группа не пустая,
            //возвращаем её (иначе создаем)
            if (group != null)
            {
                return group;
            }

            group = new mbGroup(id);
            this.AddGroup(group);

            return group;
        }

        /// <summary>
        /// Метод, вызываемый при возникновлении
        /// новой аварии
        /// </summary>
        /// <param name="alarm"></param>
        public void AddActiveAlarm(mbDiscreteAlarm alarm)
        {
            ActiveAlarms.Insert(0, alarm);
            NewAlarmNotification?.Invoke(alarm);
        }

        /// <summary>
        /// Метод, вызываемыйы при сбросе аварии
        /// </summary>
        /// <param name="alarm"></param>
        public void ResetActiveAlarm(mbDiscreteAlarm alarm)
        {
            ActiveAlarms.Remove(alarm);
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
