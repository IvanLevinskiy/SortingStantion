using System.Collections.Generic;
using System.Xml;

namespace Communication
{
    public  class mbDateTime : mbRegisterBase
    {

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbDateTime(XmlNode xmlNode)
        {
            //Длина регистра - 12 байт
            Lenght = 12;

            //Получаем свойства
            this.Address = Utilites.GetAttributInt(xmlNode, "Address");
            this.Description = Utilites.GetAttributString(xmlNode, "Description");
        }


        public mbDateTime(string address)
        {
            //Длина регистра - 12 байт
            Lenght = 12;

            //Получаем свойства
            this.Address = int.Parse(address);
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void UpdatedValue(object value)
        {
            StatusText = Status.ToString();
        }

        /// <summary>
        /// Метод для посторения значения
        /// </summary>
        /// <param name="bytes"></param>
        public override void BuildStatus(List<byte> bytes)
        {
            if (bytes.Count < 1)
            {
                return;
            }

            var year = GetNumeric(bytes);
            var month = GetNumeric(bytes);
            var day = GetNumeric(bytes);
            var hour = GetNumeric(bytes);
            var minute = GetNumeric(bytes);
            var second = GetNumeric(bytes);

            Status = string.Format("{0:00}:{1:00}:{2:00} {3:00}.{4:00}.{5:0000}", hour, minute, second, day, month, year);

            //Указываем, что данные валидны
            IsValid = true;
        }

        /// <summary>
        /// Метод по созданию ushort
        /// </summary>
        /// <returns></returns>
        short GetNumeric(List<byte> btslist)
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

            return (short)(((short)(byte_0 << 8)) | ((short)byte_1));
        }

        /// <summary>
        /// Метод для записи значения
        /// в ПЛК
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public override bool Write(object str)
        {
            return false;
        }
    }
}
