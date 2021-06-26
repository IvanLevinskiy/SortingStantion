using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communication
{
    public class mbDoubleRegister : mbRegisterBase
    {
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="address"></param>
        public mbDoubleRegister(string address)
        {
            Address = int.Parse(address);
            Lenght = 4;
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void UpdatedValue(object value)
        {
            var locstatus = Convert.ToInt32(value);
            StatusText = locstatus.ToString();
        }

        /// <summary>
        /// Метод для чтения тэга
        /// </summary>
        public override void Read()
        {

        }

        public override void BuildStatus(List<byte> bytes)
        {
            //Если данных меньше двух
            //выходим из функции
            if (bytes.Count < 4)
            {
                return;
            }

            //Построение статуса из байтов
            Status = (UInt32)(bytes[0] << 32) | (UInt32)(bytes[1] << 16) | (UInt32)(bytes[2] << 8) | ((UInt32)(bytes[3]));

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
        /// Метод, возвращающий безнаковое значение
        /// регистра
        /// </summary>
        /// <returns></returns>
        public UInt32 ToUnsigned()
        {
            return Convert.ToUInt32(Status);
        }

        /// <summary>
        /// Метод, возвращающий знаковое значение
        /// регистра
        /// </summary>
        /// <returns></returns>
        public Int32 ToSigned()
        {
            return Convert.ToInt32(Status);
        }
    }
}
