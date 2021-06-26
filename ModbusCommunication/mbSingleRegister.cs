using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace Communication
{
    /// <summary>
    /// Класс, описывающий сущность регистра
    /// типа short или usort
    /// </summary>
    public class mbSingleRegister : mbRegisterBase
    {
        /// <summary>
        /// Коллекция битов 
        /// </summary>
        public ObservableCollection<mbBit> Bits
        {
            get;
            set;
        }

        /// <summary>
        /// Значение регистра для записи
        /// </summary>
        ushort WritenValue;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbSingleRegister()
        {
            //Инициализация свойств класса
            Lenght = 2;

            //Инициализация коллекции битов
            Bits = new ObservableCollection<mbBit>();

            //Процедура для наполнения 
            //коллекции битами
            for (int i  = 0; i <16; i++ )
            {
                var bit = new mbBit(i);
                bit.Register = this;
                Bits.Add(bit);
            }

            
            SetDefaultValues();

            //Подписка на событие по потере алидности
            //данных регистром
            this.LostValid += MbSingleRegister_LostValid;
        }

        /// <summary>
        /// Метод для установки значений по 
        /// умолчанию
        /// </summary>
        void SetDefaultValues()
        {
            //Если верхняя и нижняя граница
            //значения не установлен,
            //устанавливаем её по умолчанию
            if (DataType == Types.Ushort)
            {
                SetValue(ref HightValue, 65535);
            }

            if (DataType == Types.Short)
            {
                SetValue(ref HightValue, 32767);
                SetValue(ref LowValue, -32768);
            }
        }

        /// <summary>
        /// Метод для установки значения
        /// </summary>
        /// <param name="value"></param>
        /// <param name="newvalue"></param>
        void SetValue(ref double value, double newvalue)
        {
            if (value == 0)
            {
                value = newvalue;
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbSingleRegister(int address, Types Type)
        {
            //Сохраняем тип регистра
            DataType = Type;

            //Инициализация свойств класса
            Address = address;
            Lenght = 2;

            //Установка значений по умолчанию
            SetDefaultValues();

            //Если тип данных не ushort, тогда
            //не создаем коллекцию битов
            if (DataType != Types.Ushort)
            {
                return;
            }

            //Подписка на событие по потере алидности
            //данных регистром
            this.LostValid += MbSingleRegister_LostValid;

            //Инициализация коллекции битов
            Bits = new ObservableCollection<mbBit>();

            //Процедура для наполнения 
            //коллекции битов
            for (int i = 0; i < 16; i++)
            {
                var bit = new mbBit(i);
                bit.Register = this;
                Bits.Add(bit);
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbSingleRegister(string address, Types Type)
        {
            //Сохраняем тип регистра
            DataType = Type;

            //Инициализация свойств класса
            Address = int.Parse(Utilites.ClearAddressStr(address));
            Lenght = 2;

            //Установка значений по умолчанию
            SetDefaultValues();

            //Если тип данных не ushort, тогда
            //не создаем коллекцию битов
            if (DataType != Types.Ushort)
            {
                return;
            }

            //Подписка на событие по потере алидности
            //данных регистром
            this.LostValid += MbSingleRegister_LostValid;

            //Инициализация коллекции битов
            Bits = new ObservableCollection<mbBit>();

            //Процедура для наполнения 
            //коллекции битов
            for (int i = 0; i < 16; i++)
            {
                var bit = new mbBit(i);
                bit.Register = this;
                Bits.Add(bit);
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbSingleRegister(XmlNode xmlNode)
        {
            //Инициализация свойств класса
            Lenght = 2;

            //Инициализация коллекции битов
            Bits = new ObservableCollection<mbBit>();

            //Получаем свойств
            this.Address =     Utilites.GetAttributInt(xmlNode, "Address");
            this.Description = Utilites.GetAttributString(xmlNode, "Description");

            this.HightValue = Utilites.GetAttributDouble(xmlNode, "HightValue");
            this.LowValue =   Utilites.GetAttributDouble(xmlNode, "LowValue");

            //Установка значений по умолчанию
            SetDefaultValues();

            ReadRequest = true;

            //Если тип не ushort, покидаем конструктор
            if (DataType != Types.Ushort)
            {
                return;
            }

            //Подписка на событие по потере алидности
            //данных регистром
            this.LostValid += MbSingleRegister_LostValid;

            //Процедура для наполнения 
            //коллекции битов
            for (int i = 0; i < 16; i++)
            {
                var bit = new mbBit(i);
                bit.Register = this;
                Bits.Add(bit);
            }
        }

        /// <summary>
        /// Метод, вызываемый при потере валидности
        /// данных
        /// </summary>
        private void MbSingleRegister_LostValid()
        {
            for (int i = 0; i < 16; i++)
            {
                var bit = Bits[i];
                bit.IsValid = false;
            }
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void UpdatedValue(object value)
        {

            //Если тип данных ushort
            if (DataType == Types.Ushort)
            {
                double dvalue = Convert.ToDouble(value);
                dvalue = dvalue * Factor;

                //Получение статуса
                StatusText = string.Format($"{Format}", dvalue);

                //Обновляем коллекцию битов
                Refrash();

                return;
            }

            //Если тип данных short
            if (DataType == Types.Short)
            {
                double dvalue = Convert.ToDouble(value);
                dvalue = dvalue * Factor;

                //Получение статуса
                StatusText = string.Format($"{Format}", dvalue);

                return;
            }

        }

        /// <summary>
        /// Метод для построения статуса из 
        /// принятых байт
        /// </summary>
        /// <param name="bytes"></param>
        public override void BuildStatus(List<byte> bytes)
        {
            //Если данных меньше двух
            //выходим из функции
            if (bytes.Count < 2)
            {
                return;
            }

            //Переводим данные в массив
            byte[] bytes_array = new byte[2];
            bytes_array[0] = bytes[1];
            bytes_array[1] = bytes[0];

            //Если тип данных ushort
            if (DataType == Types.Ushort)
            {
                Status = BitConverter.ToUInt16(bytes_array, 0);

                //Уведомление битов о валидности
                //данных
                for (int i = 0; i < 16; i++)
                {
                    Bits[i].IsValid = true;
                }
            }

            //Если тип данных short
            if (DataType == Types.Short)
            {
                Status = BitConverter.ToInt16(bytes_array, 0);
            }

            //Удаление использованных
            //байтов из коллекции
            bytes.RemoveAt(0);
            bytes.RemoveAt(0);

            //Указываем, что данные валидны
            IsValid = true;
        }

        /// <summary>
        /// Метод для обновления состояния
        /// битов в регистре
        /// </summary>
        void Refrash()
        {
            for (int i = 0; i < 16; i ++)
            {
                Bits[i].State = GetBitState(Bits[i].Bit);
            }
        }

        /// <summary>
        /// Метод для получения состояния бита
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        bool GetBitState(byte bit)
        {
            //Получаем слово
            var word = Convert.ToUInt16(Status);

            //Смена порядка байт
            word = ToSimaticType(word);

            UInt16 mask = (UInt16)(1 << bit);
            var temp = word & mask;
            bool bitstate = temp > 0;
            return bitstate;
        }


        /// <summary>
        /// Метод, возвращающий безнаковое значение
        /// регистра
        /// </summary>
        /// <returns></returns>
        public UInt16 ToUnsigned()
        {
            return Convert.ToUInt16(Status);
        }

        /// <summary>
        /// Метод, возвращающий знаковое значение
        /// регистра
        /// </summary>
        /// <returns></returns>
        public Int16 ToSigned()
        {
            return Convert.ToInt16(Status);
        }


        /// <summary>
        /// Метод для записи значения в ПЛК
        /// </summary>
        /// <param name="_value"></param>
        public override bool Write(object _value)
        {
            if (DataType == Types.Ushort)
            { 
                ushort uvalue = Convert.ToUInt16(_value);
                return  this.Group.Device.WriteHoldingRegister((ushort)Address, uvalue);
            }

            if (DataType == Types.Short)
            {
                short newvalue = Convert.ToInt16(_value);
                var bytes = BitConverter.GetBytes(newvalue);
                var Value = BitConverter.ToUInt16(bytes, 0);
                return this.Group.Device.WriteHoldingRegister((ushort)Address, Value);
            }

            return false;
        }

        /// <summary>
        /// Метод для записи значения в ПЛК
        /// </summary>
        /// <param name="_value"></param>
        public override bool Write(double _value)
        {
            if (DataType == Types.Ushort)
            {
                double Value = _value / Factor;

                ushort uvalue = Convert.ToUInt16(Value);

                return this.Group.Device.WriteHoldingRegister((ushort)Address, uvalue);
            }

            if (DataType == Types.Short)
            {
                double dvalue = _value / Factor;

                short svalue = Convert.ToInt16(dvalue);

                var bytes = BitConverter.GetBytes(svalue);

                var Value = BitConverter.ToUInt16(bytes, 0);

                return this.Group.Device.WriteHoldingRegister((ushort)Address, Value);
            }

            return false;
        }

    }
}
