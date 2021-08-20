using System;

namespace S7Communication
{
    public class S7_Real : simaticTagBase
    {
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7_Real(string name, string address, SimaticGroup simaticGroup)
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
                StatusText = string.Empty;
                return;
            }

            //Преобразуем значения
            var locstatus = Convert.ToDouble(value);

            //Получаем текстовый вид
            StatusText = string.Format("{0:0.00}", locstatus);
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
            Status = (float)BitConverter.ToSingle(btsarr, 0);
        }
    }
}
