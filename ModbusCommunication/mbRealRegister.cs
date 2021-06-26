using System;
using System.Collections.Generic;
using System.Xml;

namespace Communication
{
    public class mbRealRegister : mbRegisterBase
    {
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbRealRegister()
        {
            //Инициализация свойств класса
            Lenght = 4;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbRealRegister(string address)
        {
            //Инициализация свойств класса
            Address = int.Parse(address);
            DataType = Types.Float;
            Lenght = 4;

            this.HightValue = 100000f;
            this.LowValue = -100000f;

            ReadRequest = true;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbRealRegister(XmlNode xmlNode)
        {
            //Инициализация свойств класса
            Lenght = 4;

            //Получаем свойства
            this.Address = Utilites.GetAttributInt(xmlNode, "Address");
            this.Description = Utilites.GetAttributString(xmlNode, "Description");

            this.HightValue = 100000f;
            this.LowValue = -100000f;

            ReadRequest = true;
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void UpdatedValue(object value)
        {
            //Преобразуем значения
            var locstatus = Convert.ToDouble(value);

            //Округляем результат
            locstatus = Math.Round(locstatus, 5);

            if (_format != null)
            {
                //Получаем текстовый вид
                StatusText = string.Format($"{Format}", locstatus);
                return;
            }

            StatusText = locstatus.ToString();
        }

        /// <summary>
        /// Метод для получения статуса
        /// </summary>
        /// <param name="bytes"></param>
        public override void BuildStatus(List<byte> bytes)
        {
            //Если данных меньше 4 байтов
            //выходим из функции
            if (bytes.Count < 4)
            {
                return;
            }

            var btsarr = new byte[]
            {
                bytes[3],
                bytes[2],
                bytes[1],
                bytes[0],
            };

            //Построение статуса из байтов
            Status = BitConverter.ToSingle(btsarr, 0);

            //Удаление использованных
            //байтов из коллекции
            bytes.RemoveAt(0);
            bytes.RemoveAt(0);
            bytes.RemoveAt(0);
            bytes.RemoveAt(0);

            //Указываем, что данные валидны
            IsValid = true;
        }

        /// <summary>
        /// Метод для записи значения в ПЛК
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Write(object data)
        {
            //Получение массива байтов
            float value = (float)data;
            var bytes = BitConverter.GetBytes(value);

            //Получаем значение регистров
            var r1 =  (UInt16)(bytes[1] << 8) | (UInt16)(bytes[0] << 0);
            var r2 =  (UInt16)(bytes[3] << 8) | (UInt16)(bytes[2] << 0);

            //Запись регистров
            var res_1 = Group.Device.WriteHoldingRegister((ushort)Address, (ushort)r2);
            var res_2 = Group.Device.WriteHoldingRegister((ushort)(Address + 1), (ushort)r1);

            return res_1 & res_2;
        }

        /// <summary>
        /// Метод для записи значения в ПЛК
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Write(double value)
        {
            //Получение массива байтов
            float _value = (float)value;
            var bytes = BitConverter.GetBytes(_value);

            //Получаем значение регистров
            var r1 = (UInt16)(bytes[1] << 8) | (UInt16)(bytes[0] << 0);
            var r2 = (UInt16)(bytes[3] << 8) | (UInt16)(bytes[2] << 0);

            //Запись регистров
            var res_1 = Group.Device.WriteHoldingRegister((ushort)Address, (ushort)r2);
            var res_2 = Group.Device.WriteHoldingRegister((ushort)(Address + 1), (ushort)r1);

            return res_1 & res_2;
        }
    }
}
