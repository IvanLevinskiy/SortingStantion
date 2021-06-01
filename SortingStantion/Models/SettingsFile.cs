using SortingStantion.Controls;
using System;
using System.Windows;
using System.Xml;

namespace SortingStantion.Models
{
    /// <summary>
    /// Модель, реализующая файл настроек
    /// </summary>
    public class SettingsFile
    {
        public string SrvL3Url
        {
            get;
            set;
        }

        public string SrvL3Login
        {
            get;
            set;
        }

        public string SrvL3Password
        {
            get;
            set;
        }

        public string LocalSrvPort
        {
            get;
            set;
        }

        public string SrvL3UrlReport
        {
            get;
            set;
        }

        public string SerialPort232Name
        {
            get;
            set;
        }

        public string SerialPort232BaudRate
        {
            get;
            set;
        }

        public string SerialPort232Parity
        {
            get;
            set;
        }

        public string SerialPort232DataBits
        {
            get;
            set;
        }

        public string SerialPort232StopBits
        {
            get;
            set;
        }

        public string SerialPort232Handshake
        {
            get;
            set;
        }

        public string HandSerialPort232Name
        {
            get;
            set;
        }

        public string scannerServerPort
        {
            get;
            set;
        }

        public string WindowTimeOut
        {
            get;
            set;
        }

        public string LineNum
        {
            get;
            set;
        }

        public string PlcIp
        {
            get;
            set;
        }

        public string ReuestTimeout
        {
            get;
            set;
        }

        public string _1fieldPreficks
        {
            get;
            set;
        }

        public string _1fieldLength
        {
            get;
            set;
        }

        public string _2fieldPreficks
        {
            get;
            set;
        }

        public string _2fieldLength
        {
            get;
            set;
        }

        public string _3fieldPreficks
        {
            get;
            set;
        }

        public string _3fieldLength
        {
            get;
            set;
        }

        public string LogSizeInMonth
        {
            get;
            set;
        }

        public string PacketLogEnable
        {
            get;
            set;
        }

        /// <summary>
        /// Xml документ с настройками
        /// </summary>
        XmlDocument xDoc;


        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="file"></param>
        public SettingsFile(string file)
        {
            //Загрузка файла конфигурации
            Load(file);
        }

        /// <summary>
        /// Метод для загрузки файла конфигурации
        /// </summary>
        /// <param name="file"></param>
        public void Load(string file)
        {

            //Создаем экземпляр xml докумета
            xDoc = new XmlDocument();

            //Заргузка из файла
            try
            {
                xDoc.Load(file);
            }
            catch (Exception ex)
            {
                customMessageBox.Show($"Ошибка чтения файла конфигурации: {file}"); 
                return;
            }
        }

        /// <summary>
        /// Метод для получения значения из XmlNode
        /// по наименованию узла
        /// </summary>
        /// <returns></returns>
        public string GetValue(string name)
        {
            //Получаем корневой элемент
            XmlNode root = xDoc.ChildNodes[0];

            //Получаем коллекцию всех настроек
            var settingsnodes = root.ChildNodes[0];

            //Ищем нужное свойство по атрибуту Name
            foreach (XmlNode settingnode in settingsnodes)
            {

                var attr_name = GetAttribut(settingnode, "name");

                if (name == attr_name)
                {
                    var valuenode = settingnode.ChildNodes[0];
                    return valuenode.InnerText;
                }
            }

            //Возвращаем пустой результат
            return string.Empty;
        }

        /// <summary>
        /// Получение атрибута из XmlNode
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attribut"></param>
        /// <returns></returns>
        public string GetAttribut(XmlNode node, string attribut)
        {
            try
            {
                if (node.Attributes == null)
                {
                    return string.Empty;
                }

                XmlNode attributNode = node.Attributes.GetNamedItem(attribut);


                //Если атрибута нет
                if (attributNode == null)
                {
                    return string.Empty;
                }

                //Если атрибут не пустой
                if (attributNode.Value != string.Empty)
                {
                    return attributNode.Value;
                }

            }
            catch (Exception ex)
            {

            }

            return string.Empty;
        }
    }
}
