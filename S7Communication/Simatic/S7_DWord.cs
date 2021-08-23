using System;

namespace S7Communication
{
    public class S7_DWord : simaticTagBase
    {

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7_DWord(string name, string address, SimaticGroup simaticGroup)
        {
            this.Name = name;
            this.Address = address;
            this.group = simaticGroup;
            this.Lenght = 4;

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
            uint locstatus = Convert.ToUInt32(Status);
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
            var locstatus = Convert.ToInt32(value);

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
                //Status = null;
                //StatusText = string.Empty;
                return;
            }

            if (bytes.Length < offset + 4)
            {
                //Status = null;
                //StatusText = string.Empty;
                return;
            }

            //Построение статуса из байтов
            Status = (UInt32)(bytes[0 + offset] << 32) | (UInt32)(bytes[1 + offset] << 16) | (UInt32)(bytes[2 + offset] << 8) | ((UInt32)(bytes[3 + offset]));
        }

        /// <summary>
        /// Метод для записи значения в тэг
        /// </summary>
        /// <param name="value"></param>
        public override bool Write(object value)
        {
            if (value is UInt32)
            {
                UInt32 uvalue = Convert.ToUInt32(value);
                var array = BitConverter.GetBytes(uvalue);

                //Перестановка байт
                var newarray = new byte[]
                {
                        array[3], array[2], array[1], array[0]
                };

                this.device.WriteBytes(DataType, DBNumber, StartByteAddress, newarray);

                return true;
            }

            if (value is Int32)
            {
                Int32 uvalue = Convert.ToInt32(value);
                var array = BitConverter.GetBytes(uvalue);

                //Перестановка байт
                var newarray = new byte[]
                {
                        array[3], array[2], array[1], array[0]
                };

                this.device.WriteBytes(DataType, DBNumber, StartByteAddress, newarray);

                return true;
            }
           
            return true;
        }

        /// <summary>
        /// Метод для записи значения в тэг
        /// </summary>
        /// <param name="value"></param>
        public override bool Write(string value)
        {
            Int32 uvalue = Convert.ToInt32(value);
            var array = BitConverter.GetBytes(uvalue);

            //Переворачиваем байты как у симатика
            var rotaryarray = new byte[]
            {
                array[3],
                array[2],
                array[1],
                array[0],
            };


            this.device.WriteBytes(DataType, DBNumber, StartByteAddress, rotaryarray);
            return true;
        }
    }
}
