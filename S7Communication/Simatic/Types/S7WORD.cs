using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S7Communication
{
    public class S7WORD : simaticTagBase
    {
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7WORD(string name, string address, SimaticGroup simaticGroup)
        {
            this.Name = name;
            this.Address = address;
            this.group = simaticGroup;
            this.Lenght = 2;

            //Разбор адреса операнда
            ParseAddress(Address);

            //Добавление тэга в группу
            simaticGroup.Tags.Add(this);
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void ObjectToString()
        {
            uint locstatus = Convert.ToUInt16(Status);
            StatusText = locstatus.ToString();
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void UpdatedValue(object value)
        {
            //Проверка на нулевой
            //указатель
            if (value == null)
            {
                StatusText = string.Empty;
                return;
            }

            //Преобразуем значения
            var locstatus = Convert.ToInt16(value);

            //Получаем текстовый вид
            StatusText = string.Format("{0:0}", locstatus);
        }

        /// <summary>
        /// Метод для построения статуса из байт
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        public override void BuildStatus(byte[] bytes, int startbytefromrequest)
        {
            int offset = StartByteAddress - startbytefromrequest;

            //Если данных меньше 4 байтов
            //выходим из функции
            if (bytes == null)
            {
                Status = null;
                StatusText = string.Empty;
                return;
            }

            if (bytes.Length < 2)
            {
                Status = null;
                StatusText = string.Empty;
                return;
            }

            //Построение статуса из байтов
            Status = (UInt32)(bytes[0] << 8) | (UInt32)(bytes[1] << 0) ;
        }


        /// <summary>
        /// Метод для записи значения в тэг
        /// </summary>
        /// <param name="value"></param>
        public override bool Write(object value)
        {
            Int16 uvalue = Convert.ToInt16(value);
            var array = BitConverter.GetBytes(uvalue);

            //Переворачиваем байты как у симатика
            var rotaryarray = new byte[]
            {
                array[1],
                array[0],
            };


            this.device.WriteBytes(DataType, DBNumber, StartByteAddress, rotaryarray);
            return true;
        }

        /// <summary>
        /// Метод для записи значения в тэг
        /// </summary>
        /// <param name="value"></param>
        public override bool Write(string value)
        {
            Int16 uvalue = Convert.ToInt16(value);
            var array = BitConverter.GetBytes(uvalue);

            //Переворачиваем байты как у симатика
            var rotaryarray = new byte[]
            {
                array[1],
                array[0],
            };


            this.device.WriteBytes(DataType, DBNumber, StartByteAddress, rotaryarray);
            return true;
        }
    }
}