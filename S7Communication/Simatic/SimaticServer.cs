using System;
using System.Xml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using S7Communication.Utilites;

namespace S7Communication
{
    public class SimaticServer : INotifyPropertyChanged
    {

        #region MVVM

        /// <summary>
        /// Флаг, указывающий на то,
        /// выбрано ли устройство или нет
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

                //Ищем выбранный узел
                FindeSelectedElement();
            }
        }
        bool _isSelected;

        /// <summary>
        /// Флаг, указывающий на то, 
        /// раскрыт ли узел в дереве проекта
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
        /// Элемент, выбраный в дереве
        /// проекта
        /// </summary>
        public object SelectElement
        {
            get
            {
                return selectElement;
            }
            set
            {
                selectElement = value;
                OnPropertyChanged("SelectElement");
            }
        }
        object selectElement;

        #endregion

        /// <summary>
        /// Свойство для запуска и
        /// остановки сервера
        /// </summary>
        public bool IsRun
        {
            get
            {
                return _isrun;
            }
            set
            {
                _isrun = value;
                OnPropertyChanged("IsRun");
            }
        }
        bool _isrun;

        /// <summary>
        /// Коллекция устройств, входящих в состав сервера
        /// </summary>
        public ObservableCollection<SimaticDevice> Devices
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
        ObservableCollection<SimaticDevice> devices;


        /// <summary>
        /// Список тэгов
        /// </summary>
        public ObservableCollection<simaticTagBase> Tags
        {
            get
            {
                var alltags = new ObservableCollection<simaticTagBase>();
                foreach (var device in Devices)
                {
                    var tags = device.Tags;

                    foreach (var tag in tags)
                    {
                        alltags.Add(tag);
                    }
                }

                return alltags;
            }
        }    

       /// <summary>
       /// Количество тэгов в сервера
       /// </summary>
        public int QuantityTags 
        {
            get; 
            set; 
        }

        /// <summary>
        /// Таймаут между опросами
        /// </summary>
        int timeout = 300;
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
                OnPropertyChanged("Timeout");
            }
        }

        #region КОНСТРУКТОРЫ
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public SimaticServer()
        {
            this.Devices = new ObservableCollection<SimaticDevice>();
        }

        /// <summary>
        /// Конструктор класса их XML
        /// </summary>
        public SimaticServer(string patch)
        {
            Devices = new ObservableCollection<SimaticDevice>();
            LoadServer(patch);
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
                    this.AddDevice(new SimaticDevice(deviceNode));
                }
            }
        }

        /// <summary>
        /// Метод для открытия файла
        /// </summary>
        /// <param name="oldpath"></param>
        /// <returns></returns>
        public string OpenBrouse(string oldpath)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            if (of.ShowDialog() == true)
            {
                return of.FileName;
            }

            return oldpath;
        }

        #endregion

        /// <summary>
        /// Метод для добавления нового устройства
        /// </summary>
        /// <param name="device"></param>
        public void AddDevice(SimaticDevice device)
        {
            device.server = this;
            this.Devices.Add(device);
        }

        /// <summary>
        /// Метод для добавления нового устройства
        /// </summary>
        /// <param name="device"></param>
        public void AddDevice(ObservableCollection<SimaticDevice> device)
        {
            for (int i = 0; i < device.Count; i++)
            {
                this.Devices.Add(device[i]);
            }
        }

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

            IsRun = true;
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


            IsRun = false;

        }

        /// <summary>
        /// Метод для однократного чтения тэгов
        /// </summary>
        public void Read()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var device in Devices)
                {

                    //SimaticQueryData sqd = new SimaticQueryData(Converter.CollectionToArray(device.SelectedTags), device);

                    //while (sqd.Execute())
                    //{

                    //}
                }
            });
        }



        #region События при изменении выбранного тэга


        /// <summary>
        /// Заполнение коллекции выбранного элемента
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public ObservableCollection<simaticTagBase> GetTags(object element)
        {
            ObservableCollection<simaticTagBase> tags = new ObservableCollection<simaticTagBase>();

            //Если элемент сервер
            if (element is SimaticServer)
            {
                foreach (SimaticDevice device in ((SimaticServer)element).Devices)
                {
                    foreach (SimaticGroup group in device.Groups)
                    {
                        foreach (simaticTagBase tag in group.Tags)
                        {
                            tags.Add(tag);
                        }
                    }
                }

                return tags;
            }

            //Если элемент устройство
            if (element is SimaticDevice)
            {
                foreach (SimaticGroup group in ((SimaticDevice)element).Groups)
                {
                    foreach (simaticTagBase tag in group.Tags)
                    {
                        tags.Add(tag);
                    }
                }

                return tags;
            }

            //Если элемент группа
            if (element is SimaticGroup)
            {
                foreach (simaticTagBase tag in ((SimaticGroup)element).Tags)
                {
                    tags.Add(tag);
                }

                return tags;
            }

            //Если элемент Тэг
            if (element is simaticTagBase)
            {
                tags.Add((simaticTagBase)element);
            }

            return tags;
        }

        #endregion



        /// <summary>
        /// Метод для поиска
        /// выбранного элемента в сервере (для MVVM)
        /// </summary>
        public void FindeSelectedElement()
        {
            //Если выбран Сервер
            if (this.IsSelected)
            {
                SelectElement = this;
                return;
            }

            foreach (SimaticDevice device in Devices)
            {
                if (device.IsSelected)
                {
                    SelectElement = device;
                    return;
                }

                foreach (SimaticGroup group in device.Groups)
                {
                    if (group.IsSelected)
                    {
                        SelectElement = group;
                        return;
                    }

                    foreach (simaticTagBase _tag in group.Tags)
                    {
                        if (_tag.IsSelected)
                        {
                            SelectElement = _tag;
                            return;
                        }
                    }
                }
            }

            //Если ничего не нашли, тогда пишем нуль в переменную
            SelectElement = null;
        }

        /// <summary>
        /// Чтение значений
        /// </summary>
        public ICommand ReadCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Read();
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
