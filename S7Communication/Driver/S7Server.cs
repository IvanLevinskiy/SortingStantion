using System;
using Simatic.Driver;
using System.Xml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace S7Communication.Driver
{
    public class S7Server : INotifyPropertyChanged
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
        public ObservableCollection<S7Device> Devices
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
        ObservableCollection<S7Device> devices;


       /// <summary>
       /// Количество тэгов в сервера
       /// </summary>
        public int QuantityTags 
        {
            get; 
            set; 
        }   


        //3. Всплывающая подсказка
        public string ToolTipText;

        #region КОНСТРУКТОРЫ
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7Server()
        {
            this.Devices = new ObservableCollection<S7Device>();
        }

        /// <summary>
        /// Конструктор класса их XML
        /// </summary>
        public S7Server(string patch)
        {
            Devices = new ObservableCollection<S7Device>();
            LoadServer(patch);
        }

        /// <summary>
        /// Конструктор сервера из xml ПЕРЕГРУЗКА 2
        /// </summary>
        /// <param name="patch"></param>
        //void LoadServer(string patch)
        //{
        //    XmlDocument xDoc = new XmlDocument();

        //    try
        //    {
        //        xDoc.Load(patch);

        //        //Получим корневой элемент
        //        XmlElement server = xDoc.DocumentElement;

        //        //Обход всех узлов в корневом элементе TagManager
        //        foreach (XmlNode DeviceNode in server)
        //        {
        //            //Создание Device из XmlNode
        //            S7Device device = new S7Device(DeviceNode);

        //            // Обходим все дочерние узлы элемента Device
        //            foreach (XmlNode GroupNode in DeviceNode.ChildNodes)
        //            {

        //                // если узел - Group
        //                if (GroupNode.Name == "Group")
        //                {
        //                    SimaticGroup group = new SimaticGroup(device, GroupNode);

        //                    //Проходим по вложенным узлам
        //                    foreach (XmlNode TagNode in GroupNode.ChildNodes)
        //                    {
        //                        if (TagNode.Name == "Name")
        //                        {
        //                            group.Name = TagNode.InnerText;
        //                        }

        //                        if (TagNode.Name == "Tag")
        //                        {
        //                            //SimaticTag tag = new SimaticTag(TagNode);

        //                            //Подсчет количества тэгов
        //                            group.QuantityTags++;
        //                            device.QuantityTags++;
        //                            QuantityTags++;

        //                            //tag.ParentGroup = group;

        //                            //Создаем полное имя тэга нпр PLC.Group.Tag
        //                            //tag.FullName = string.Format("{0}.{1}.{2}", device.Name, group.Name, tag.Name);

        //                            //group.AddTag(tag);
        //                        }
        //                    }

        //                    device.AddGroup(group);
        //                }
        //            }

        //            AddDevice(device);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

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
                return;
            }



            //Получение корневого элемента
            XmlElement server = xDoc.DocumentElement;

            //Обход всех узлов в корневом элементе
            foreach (XmlNode deviceNode in server)
            {
                if (deviceNode.Name.ToUpper() == "DEVICE")
                {
                    //this.AddDevice(new S7Device(deviceNode));
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
        public void AddDevice(S7Device device)
        {
            device.Server = this;
            this.Devices.Add(device);
        }

        /// <summary>
        /// Метод для добавления нового устройства
        /// </summary>
        /// <param name="device"></param>
        public void AddDevice(ObservableCollection<S7Device> device)
        {
            for (int i = 0; i < device.Count; i++)
            {
                this.Devices.Add(device[i]);
            }
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

        /// <summary>
        /// Метод для однократного чтения тэгов
        /// </summary>
        /// <param name="device"></param>
        public void Read(S7Device device)
        {
            var res = device.Open();
            if (res != ErrorCode.NoError)
            {

            }

            foreach (var group in device.Groups)
            {
                foreach (var tag in group.Tags)
                {
                    for (int i = 0; i < 3; i++)
                    {
                      
                    }

                }
            }
        }

        /// <summary>
        /// Метод для однократного чтения тэгов
        /// </summary>
        /// <param name="group"></param>
        //public void Read(SimaticGroup group)
        //{
        //    Task.Factory.StartNew(() =>
        //    {
        //        SimaticQueryData sqd = new SimaticQueryData(Converter.CollectionToArray(group.Tags),  group.ParentDevice);

        //        while (sqd.Execute())
        //        {

        //        }
        //    });
        //}


    



      
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
