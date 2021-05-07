using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml;
using System.Threading.Tasks;

namespace S7Communication.Driver
{
    //ГРУППА ТЭГОВ
    public class S7Group : INotifyPropertyChanged
    {
        /// <summary>
        /// Уникальный идентефикатор
        /// </summary>
        public int ID;

        /// <summary>
        /// Символьное имя
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
        /// Коллекция тэгов
        /// </summary>
        public ObservableCollection<simaticTagBase> Tags
        {
            get;
            set;
        }

        /// <summary>
        /// Путь в сервере
        /// </summary>
        public string FullPatch
        {
            get;
            set;
        }


        delegate void function();
        function funct;


        /// <summary>
        /// Поле, содержащее указатель
        /// на родительский объект SimaticDevice
        /// </summary>
        public S7Device s7Device;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7Group()
        {
            Tags = new ObservableCollection<simaticTagBase>();
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="Name">Символьное имя</param>
        public S7Group(string Name)
        {
            this.Name = Name;
            Tags = new ObservableCollection<simaticTagBase>();
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="_Tags">Коллекция тэгов</param>
        public S7Group(ObservableCollection<simaticTagBase> _Tags)
        {
            Tags = _Tags;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="Tags">Массив тэгов</param>
        public S7Group(simaticTagBase[] Tags)
        {
            this.Tags = new ObservableCollection<simaticTagBase>();
            this.Tags.Clear();

            for (int i = 0; i < Tags.Length; i++)
            {
                this.Tags.Add(Tags[i]);
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="Name">символьное имя</param>
        /// <param name="Tags">Коллекция тэгов</param>
        public S7Group(string Name, ObservableCollection<simaticTagBase> Tags)
        {
            this.Name = Name;
            this.Tags = Tags;
        }

        
        /// <summary>
        /// Конструктор для получения устройства из XmlNode
        /// </summary>
        /// <param name="node">Объект типа XmlNode</param>
        public S7Group(S7Device sDevice, XmlNode node)
        {
            //Инициализация коллекции тэгов
            this.Tags = new ObservableCollection<simaticTagBase>();

            //Указатель на родительское устройство
            s7Device = sDevice;

            //Получение имени группы
            //Name = Utilites.Converter.XmlNodeToString(node, "Name");


            //Наполнение коллекции тэгами
            foreach (XmlNode tagnode in node.ChildNodes)
            {
                var tag = CreateTag(tagnode);
                Tags.Add(tag);
            }
        }

        /// <summary>
        /// Метод для создания регистра
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        simaticTagBase CreateTag(XmlNode xmlNode)
        {

            //Получаем свойства тэга
            //var type =    Utilites.Converter.XmlNodeToString(xmlNode, "Type").ToUpper();
            //var address = Utilites.Converter.XmlNodeToString(xmlNode, "Address");
            //var name =    Utilites.Converter.XmlNodeToString(xmlNode, "Name");

            //Создаем регистр
            simaticTagBase tag = null;

            //Управляем моделью создания регистра
            //switch (type)
            //{
            //    //case "BOOL":  tag = new mbSingleRegister(address, Types.Short); break;
            //    //case "BYTE": tag = new mbSingleRegister(address, Types.Ushort); break;
            //    //case "WORD":   tag = new mbRealRegister(address); break;
            //    case "DWORD":  tag = new simaticDWord(name, address, this); break;
            //    case "REAL":  tag = new simaticReal(name, address, this); break;
            //}

            //возвращаем регистр
            return tag;
        }

        /// <summary>
        /// Добавление тэга в группу
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public void AddTag(simaticTagBase tag)
        {
            tag.ParentGroup = this;
            this.Tags.Add(tag);
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
