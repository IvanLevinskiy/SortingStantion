using System;

namespace S7Communication
{
    public class S7_Boolean : simaticTagBase
    {
        /// <summary>
        /// Байт, в котором хранится
        /// значение бита
        /// </summary>
        byte statusByte
        {
            get;
            set;
        }

        /// <summary>
        /// Бит, в котором хранится
        /// байт
        /// </summary>
        int  Bit
        {
            get;
            set;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7_Boolean(string name, string address, SimaticGroup simaticGroup)
        {
            this.Name = name;
            this.Address = address;

            this.group = simaticGroup;

            //Количество байт, занимаемых в DB
            Lenght = 1;

            //Получение бита операнда
            var sarray = Address.Split('.');
            var sbit = sarray[sarray.Length - 1];
            Bit = Convert.ToInt16(sbit);

            //Добавление тэга в группу
            simaticGroup.AddTag(this);
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void ObjectToString()
        {
            bool locstatus = Convert.ToBoolean(Status);
            StatusText = locstatus.ToString();
        }

        /// <summary>
        /// Метод для записи значения в ПЛК
        /// </summary>
        /// <param name="value"></param>
        public override bool Write(object value)
        {
            
            //Приводим записываемое значение к типу bool 
            var _value = Convert.ToBoolean(value);

            //Читаем состояние байта из ПЛК
            var report = device.ReadBytesWithASingleRequest(this.DataType, this.DBNumber, this.StartByteAddress, 1);

            if (report == null)
            {
                return false;
            }

            var oldsatus = report[0];

            //Новое значение
            var newvalue = oldsatus;

            //Алгоритм установки бита
            if (_value == true)
            {
                byte mask = (byte)((byte)(1 << Bit));
                newvalue |= (byte)mask;

                var array = new byte[]
                {
                    newvalue
                };

                var result = device.WriteBytes(DataType, DBNumber, StartByteAddress, array);

                //Если запись успешна - обновляем статус
                //if (result == true)
                //{
                //    Status = value;
                //}

                return result;
            }

            //Алгоритм сброса бита
            if (_value == false)
            {
                byte mask = (byte)(~(byte)(1 << Bit));
                newvalue &= (byte)mask;

                var array = new byte[]
                {
                    newvalue
                };

                var result = device.WriteBytes(DataType, DBNumber, StartByteAddress, array);

                //Если запись успешна - обновляем статус
                if (result == true)
                {
                    Status = value;
                }

                return result;
            }

            return false;
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
            var locstatus = Convert.ToBoolean(value);

            //Получаем текстовый вид
            StatusText = string.Format("{0}", locstatus);
        }

        /// <summary>
        /// Метод для получения состояния бита
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        bool GetBitState()
        {
            //Смена порядка байт
            //word = ToSimaticType(word);

            byte mask = (byte)(1 << Bit);
            var temp = statusByte & mask;
            bool bitstate = temp > 0;
            return bitstate;
        }

        /// <summary>
        /// Метод для построения статуса из байт
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        public override void BuildStatus(byte[] bytes, int startbytefromrequest)
        {

            int offset = StartByteAddress - startbytefromrequest;

            var v =  this.Address;

            //Если отсутсвует указатель
            //на массив байт - выходим
            if (bytes == null)
            {
                //Status = null;
                //StatusText = string.Empty;
                return;
            }

            //Если данных меньше 1 байта
            //выходим из функции
            if (bytes.Length < 1)
            {
                //Status = null;
                //StatusText = string.Empty;
                return;
            }

            //Построение статуса из байтов
            statusByte = bytes[offset];
            Status = GetBitState();
        }
    }
}
