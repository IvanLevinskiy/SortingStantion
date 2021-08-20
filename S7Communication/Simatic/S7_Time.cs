using System;

namespace S7Communication
{
    public class S7_Time : simaticTagBase
    {
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7_Time(string name, string address, SimaticGroup simaticGroup)
        {
            this.Name = name;
            this.Address = address;
            this.group = simaticGroup;
            this.Lenght = 4;

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
                //StatusText = string.Empty;
                return;
            }

            //Преобразуем значения
            var locstatus = Convert.ToDouble(value);

            //Получаем текстовый вид
            StatusText = string.Format("{0:0}", locstatus);
        }

        /// <summary>
        /// Метод для построения статуса из байт
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startbytefromrequest"></param>
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

            if (bytes.Length < 4)
            {
                Status = null;
                StatusText = string.Empty;
                return;
            }

            var btsarr = new byte[]
            {
                bytes[offset + 3],
                bytes[offset + 2],
                bytes[offset + 1],
                bytes[offset + 0],
            };

            //Построение статуса из байтов
            Status = BitConverter.ToInt32(btsarr, 0);
        }

        /// <summary>
        /// Метод для записи значения в тэг
        /// </summary>
        /// <param name="value"></param>
        public override bool Write(object value)
        {
            UInt32 uvalue = Convert.ToUInt32(value);
            var array = BitConverter.GetBytes(uvalue);
            this.device.WriteBytes(DataType, DBNumber, StartByteAddress, array);
            return true;
        }

        /// <summary>
        /// Метод для записи значения в тэг
        /// </summary>
        /// <param name="value"></param>
        public override bool Write(string value)
        {
            //Преобразование с обработкой ошибки
            //ввода
            UInt32 uvalue = 0;
            var result = UInt32.TryParse(value, out uvalue);

            //Обработка ошибки ввода
            if (result == false)
            {
                return false;
            }

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
