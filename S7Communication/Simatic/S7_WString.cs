using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S7Communication
{
    public class S7_WString : simaticTagBase
    {
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7_WString(string name, string address, SimaticGroup simaticGroup)
        {
            this.Name = name;
            this.Address = address.Split('-')[0];
            this.group = simaticGroup;
            this.Lenght = GetLenght(address);

            //Добавление тэга в группу
            simaticGroup.Tags.Add(this);
        }

        /// <summary>
        /// Метод для получения длины строки
        /// </summary>
        /// <param name="s7operand"></param>
        /// <returns></returns>
        int GetLenght(string s7operand)
        {
            var str = s7operand.Replace("STR", "\n");
            str = str.Split('\n')[1];
            return int.Parse(str) * 2 + 4;
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void ObjectToString()
        {
            StatusText = Status.ToString();
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

            //Получаем текстовый вид
            StatusText = Status.ToString();
        }

        /// <summary>
        /// Метод для построения статуса из байт
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startbytefromrequest"></param>
        public override void BuildStatus(byte[] bytes, int startbytefromrequest)
        {
            int offset = StartByteAddress - startbytefromrequest;

            //Если указателя на массив байт 
            //ответа отсутсвует, выходим
            if (bytes == null)
            {
                return;
            }

            //Если дины байь ответа недостаточно - выходим
            if (bytes.Length < offset + Lenght)
            {
                return;
            }

           
            //Собираем строку
            List<byte> bytes_array = new List<byte>();

            //Получаем длину сообщения из строки
            var stringlenght = bytes[offset + 3];

            //Индекс первого символа
            var firstSymbolIndex = offset + 4;

            //Индекс последнего символа
            var lastSymbolIndex = firstSymbolIndex + stringlenght * 2;

            byte lastsymbol = 0;

            //Переносим данные из списка в массив
            for (int i = firstSymbolIndex; i < lastSymbolIndex; i++)
            {
                if (bytes.Length < i)
                {
                    return;
                }

                var symbol = bytes[i];

                if (symbol == 4)
                {
                    symbol = 209;
                }

                if (lastsymbol == 209)
                {
                    symbol += 64;
                }

                bytes_array.Add(symbol);
                lastsymbol = symbol;
            }

            //Получаем строку из байтов
            string str = Encoding.UTF8.GetString(bytes_array.ToArray());
            str = str.Replace("\0", "");
            Status = StatusText = str;
        }

        /// <summary>
        /// Метод для записи значения в тэг
        /// </summary>
        /// <param name="value"></param>
        public override bool Write(object value)
        {
            //Переводим данные в строку
            var data = value.ToString();

            //Получаем байты из строки
            var bts = Encoding.GetEncoding(1251).GetBytes(data);

            //Переводим данные в список
            List<byte> btslist = new List<byte>(Lenght);
            foreach (var b in bts)
            {
                btslist.Add(b);
            }

            //Вставляем длину строки
            btslist.Insert(0, (byte)btslist.Count);

            bool result = true;

            //Записываем значения в ПЛК
            this.device.WriteBytes(DataType, DBNumber, StartByteAddress + 1, btslist.ToArray());

            return result;
        }

        /// <summary>
        /// Метод для записи значения в тэг
        /// </summary>
        /// <param name="value"></param>
        public override bool Write(string value)
        {
            //Переводим данные в строку
            var data = value;

            //Получаем байты из строки
            var bts = Encoding.GetEncoding(1251).GetBytes(data);

            //Переводим данные в список
            List<byte> btslist = new List<byte>(Lenght);
            foreach (var b in bts)
            {
                btslist.Add(b);
            }

            //Вставляем длину строки
            btslist.Insert(0, (byte)btslist.Count);

            bool result = true;

            //Записываем значения в ПЛК
            this.device.WriteBytes(DataType, DBNumber, StartByteAddress + 1, btslist.ToArray());

            return result;
        }

    }
}
