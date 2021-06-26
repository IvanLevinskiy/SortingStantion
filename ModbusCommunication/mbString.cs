using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Communication
{
    /// <summary>
    /// Класс, реализующий работу со строками
    /// хранящимися в регистрах
    /// </summary>
    public class mbString : mbRegisterBase
    {
        /// <summary>
        /// Текст из массива
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }
        string text;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbString(int firstregister, int lenght)
        {
            //Получаем длину (в байтах)
            Lenght = lenght;


            //Получаем свойства
            this.Address = firstregister;
            ReadRequest = true;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbString(XmlNode xmlNode)
        {
            //Получаем длину (в байтах)
            Lenght = Utilites.GetAttributInt(xmlNode, "Lenght");

            if (Lenght < 26)
            {
                Lenght = 26;
            }

            //Получаем свойства
            this.Address = Utilites.GetAttributInt(xmlNode, "Address");
            this.Description = Utilites.GetAttributString(xmlNode, "Description");
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void UpdatedValue(object value)
        {
            StatusText = Text;
        }


        public override void BuildStatus(List<byte> bytes)
        {
            if (bytes.Count < 1)
            {
                return;
            }

            //Указываем, что данные валидны
            IsValid = true;

            //Собираем строку
            List<byte> bytes_array = new List<byte>();

            //Переносим данные из списка в массив
            for (int i = 0; i < Lenght; i++)
            {
                if (bytes.Count < 1)
                {
                    return;
                }

                bytes_array.Add(bytes[0]);
                bytes.RemoveAt(0);
            }

            //Получаем строку из байтов
            string str = Encoding.GetEncoding(1251).GetString(bytes_array.ToArray());
            str = str.Replace("\0", "");
            Status = Text = str;
        }

        /// <summary>
        /// Метод по созданию ushort
        /// </summary>
        /// <returns></returns>
        ushort CreatUSHORT(List<byte> btslist)
        {
            byte byte_0 = 0;
            byte byte_1 = 0;

            if (btslist.Count > 0)
            {
                byte_0 = btslist[0];
                btslist.RemoveAt(0);
            }

            if (btslist.Count > 0)
            {
                byte_1 = btslist[0];
                btslist.RemoveAt(0);
            }

            return (ushort)(((ushort)(byte_0 << 8)) | ((ushort)byte_1));
        }

        public override bool Write(object str)
        {
            var data = str.ToString();

            //Получаем байты из строки
            var bts = Encoding.GetEncoding(1251).GetBytes(data);

            //Переводим данные в список
            List<byte> btslist = new List<byte>(Lenght);
            foreach (var b in bts)
            {
                btslist.Add(b);
            }

            int regid = Address;

            bool result = true;

            //Записываем значения в регистр
            for (int i = 0; i < Lenght/2; i ++)
            {
                ushort value = CreatUSHORT(btslist);
                var res = this.Group.Device.WriteHoldingRegister((ushort)(Address + i), value);
                result &= res; 
            }

            return result;
        }
    }
}
