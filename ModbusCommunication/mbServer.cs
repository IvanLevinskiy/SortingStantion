using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml;

namespace Communication
{
    public class mbServer : INotifyPropertyChanged
    {
        /// <summary>
        /// Коллекция устройств
        /// </summary>
        public ObservableCollection<mbDevice> Devices
        {
            get
            {
                return devices;
            }
            set
            {
                devices = value;
            }
        }
        ObservableCollection<mbDevice> devices;

        /// <summary>
        /// Таймаут между опросом
        /// </summary>
        public int Timeout = 100;

        /// <summary>
        /// Количество регистров
        /// </summary>
        public int QuantityRegistrs 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Коллекция активных аварий
        /// </summary>
        public ObservableCollection<mbDevice> ActiveAlarms
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
        ObservableCollection<mbDevice> active_alarms = new ObservableCollection<mbDevice>();

        /// <summary>
        /// Флаг, указывающий на то, запущен ли сервер
        /// </summary>
        bool isRun = false;

        /// <summary>
        /// Событие, вохникаемое при загрузке
        /// сервера из файла xml
        /// </summary>
        public event Action ServerIsLoaded;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbServer()
        {
            Devices = new ObservableCollection<mbDevice>();
        }

        /// <summary>
        /// Конструктор класса из xml
        /// </summary>
        /// <param name="path"></param>
        public mbServer(string path)
        {
            Devices = new ObservableCollection<mbDevice>();

            LoadServer(path);

            //Уведомление подписчиков
            //о том, что конфигурация сервера загружена
            ServerIsLoaded?.Invoke();
        }

        /// <summary>
        /// Метод для загрузки сервера из xml
        /// </summary>
        /// <param name="path"></param>
        void LoadServer(string path)
        {
            //Создание экземпляра документа
            XmlDocument xDoc = new XmlDocument();

            //Заргузка из файла
            try
            {
                xDoc.Load(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            

            //Получение корневого элемента
            XmlElement server = xDoc.DocumentElement;

            //Обход всех узлов в корневом элементе
            foreach (XmlNode deviceNode in server)
            {
                if (deviceNode.Name.ToUpper() == "DEVICE")
                {
                    this.AddDevice(new mbDevice(deviceNode));
                }
            }      
        }

        /// <summary>
        /// Диспетчер UI
        /// </summary>
        public Dispatcher UI_Ddispather;


        /// <summary>
        /// Метод для запуска сервера
        /// </summary>
        public void Start()
        {
            foreach (var device in Devices)
            {
                device.ProccesStream = new Thread(device.loop);
                device.ProccesStream.IsBackground = true;
                device.ProccesStream.Start();
            }

            isRun = true;
        }

        /// <summary>
        /// Метод для остановки сервера
        /// </summary>
        public void Stop()
        {
            foreach (var device in Devices)
            {
                if (device.ProccesStream.IsAlive == true)
                {
                    device.ProccesStream.Suspend();
                }

                device.ProccesStream = null;
            }

            isRun = false;

        }
       
        /// <summary>
        /// Метод для добавления нового устройства
        /// </summary>
        /// <param name="device"></param>
        public void AddDevice(mbDevice device)
        {
            device.Server = this;
            this.Devices.Add(device);
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
        /// Метод для получения экземпляра бита
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public mbBit GetBit(string Address, bool readrequest = false)
        {
            //Получение компанентов адреса ПОРТ:РЕГИСТР.БИТ
            var components = Address.Split(':');

            var port = Convert.ToInt16(components[0]);
            var reg_id = Convert.ToInt16(components[1].Split('.')[0]);
            var bit_id = Convert.ToInt16(components[1].Split('.')[1]);

            //Устройство
            mbDevice device = GetDeviceByPort(port);

            //Регистр
            var register = (mbSingleRegister)GetRegisterByAddress($"{port}:{reg_id}-U");
            register.ReadRequest = readrequest;

            //Возвращаем бит
            return register.Bits[bit_id];
        }


        /// <summary>
        /// Метоод для получения экземпляра 
        /// устройства по номеру порта
        /// </summary>
        public mbDevice GetDeviceByPort(int port)
        {
            mbDevice device = null;

            //Поиск устройства по порту
            for (int i = 0; i < Devices.Count; i++)
            {
                if (port == Devices[i].Port)
                {
                   return Devices[i];
                }
            }

            return device;        
        }

        /// <summary>
        /// Метод для поиска регистра в сервере
        /// (если регистра нет, добавляем его)
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public mbRegisterBase GetRegisterByAddress(string Address)
        {
            //TCP порт
            var port = Convert.ToInt16(Utilites.GetPortStr(Address));
            
            //Получение устройства
            mbDevice device = GetDeviceByPort(port);

            //Если устройство не найдено, возвращаем null
            if (device == null)
            {
                return null;
            }

            //Получение регистра
            return device.GetRegisterByAddress(Address);
        }

        /// <summary>
        /// Метод для поиска регистра в сервере
        /// а в случае его отсутсвия создания регистра
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public mbRegisterBase FingOrCreatRegisterByAddress(string Address)
        {
            //Получение компанентов адреса ПОРТ:РЕГИСТР.БИТ
            var components = Address.Split(':');

            //TCP порт
            var port = Convert.ToInt16(components[0]);

            //Получение устройства
            mbDevice device = GetDeviceByPort(port);

            //Если устройство не найдено, возвращаем null
            if (device == null)
            {
                return null;
            }

            var reg_id = Convert.ToInt16(components[1]);

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

            //Если регистр не нашли создаем его
            mbSingleRegister _register = new mbSingleRegister();

            mbGroup _group = device.FindOrCreateGroupByID("Other");

            _group.AddRegisrt(_register);



            return _register;

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
