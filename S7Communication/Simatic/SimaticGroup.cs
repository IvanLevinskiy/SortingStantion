﻿using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml;

namespace S7Communication
{

    //ГРУППА ТЭГОВ
    public class SimaticGroup : INotifyPropertyChanged
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
        /// Полное имя
        /// </summary>
        public string FullName
        {
            get
            {
                return $"{ParentDevice.Name}.{Name}";
            }
        }

        /// <summary>
        /// Коллекция тэгов
        /// </summary>
        public ObservableCollection<simaticTagBase> Tags
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
        public SimaticDevice ParentDevice;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public SimaticGroup()
        {
            Tags = new ObservableCollection<simaticTagBase>();
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="Name">Символьное имя</param>
        public SimaticGroup(string Name)
        {
            this.Name = Name;
            Tags = new ObservableCollection<simaticTagBase>();
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="_Tags">Коллекция тэгов</param>
        public SimaticGroup(ObservableCollection<simaticTagBase> _Tags)
        {
            Tags = _Tags;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="Tags">Массив тэгов</param>
        public SimaticGroup(simaticTagBase[] Tags)
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
        public SimaticGroup(string Name, ObservableCollection<simaticTagBase> Tags)
        {
            this.Name = Name;
            this.Tags = Tags;
        }


        /// <summary>
        /// Конструктор для получения устройства из XmlNode
        /// </summary>
        /// <param name="node">Объект типа XmlNode</param>
        public SimaticGroup(SimaticDevice sDevice, XmlNode node)
        {
            //Инициализация коллекции тэгов
            this.Tags = new ObservableCollection<simaticTagBase>();

            //Указатель на родительское устройство
            ParentDevice = sDevice;

            //Получение имени группы
            Name = Utilites.Converters.XmlNodeToString(node, "Name");


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
            var type = Utilites.Converters.XmlNodeToString(xmlNode, "Type").ToUpper();
            var address = Utilites.Converters.XmlNodeToString(xmlNode, "Address");
            var name = Utilites.Converters.XmlNodeToString(xmlNode, "Name");

            //Создаем регистр
            simaticTagBase tag = null;

            //Управляем моделью создания регистра
            switch (type)
            {
                //case "BOOL":  tag = new mbSingleRegister(address, Types.Short); break;
                //case "BYTE": tag = new mbSingleRegister(address, Types.Ushort); break;
                //case "WORD":   tag = new mbRealRegister(address); break;
                case "DWORD": tag = new S7_DWord(name, address, this); break;
                case "REAL": tag = new S7_Real(name, address, this); break;
            }

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
            tag.group = this;
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