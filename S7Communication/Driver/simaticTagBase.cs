using Simatic.Driver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace S7Communication.Driver
{
    public class simaticTagBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Имя тэга
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        string name;


        /// <summary>
        /// Адрес тэга
        /// </summary>
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
                temporyAddress = value;
                ParseAddress(address);
                OnPropertyChanged("Address");
            }
        }
        string address;

        /// <summary>
        /// Комментарий
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;

                OnPropertyChanged("Description");

            }
        }
        string description;

        /// <summary>
        /// Статус тэга (string)
        /// </summary>
        public string StatusText
        {
            get
            {
                return statusText;
            }
            set
            {
                statusText = value;
                OnPropertyChanged("StatusText");
            }
        }
        string statusText = string.Empty;

        /// <summary>
        /// Статус тэга
        /// </summary>
        public object Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                ObjectToString();
            }
        }
        object status = 0;

        /// <summary>
        /// Список тэгов (костыль для MVVM)
        /// </summary>
        public ObservableCollection<simaticTagBase> Tags
        {
            get
            {
                var tags = new ObservableCollection<simaticTagBase>();
                tags.Add(this);
                return tags;
            }
        }


        /// <summary>
        /// Тип данных ПЛК
        /// </summary>
        public DataType DataType
        {
            get;
            set;
        }

        /// <summary>
        /// Номер DB, в которой находится тэг.
        /// Емли переменная не в DB, тогда DBNumber = 0
        /// </summary>
        public int DBNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Начальный байт в памяти ПЛК, где хранится тэг
        /// </summary>
        public int StartByteAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Переменная, позволяющая разобрать адрес
        /// </summary>
        private string temporyAddress;

        
        /// <summary>
        /// Начальный байт в телеграмме
        /// </summary>
        public int StartByteFromTelegramm;

        /// <summary>
        /// Конечный байт в телеграмме
        /// </summary>
        public int EndByteFromTelegramm;

      

        /// <summary>
        /// Длина в байтах
        /// </summary>
        public int Lenght
        {
            get;
            set;
        }
        
        /// <summary>
        /// Старый статус переменной
        /// </summary>
        public object OldStatus = null;

        /// <summary>
        /// Родительская группа
        /// </summary>
        public S7Group ParentGroup
        {
            get
            {
                return parentGroup;
            }
            set
            {
                parentGroup = value;
            }
        }
        S7Group parentGroup = new S7Group();

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public simaticTagBase()
        {
            
        }

        /// <summary>
        /// Метод для разбора адреса
        /// </summary>
        private void ParseAddress(string address)
        {
            //Получение типа данных
            //DataType = Converter.StringToDataType(address);

            ////Получение номера блока данных
            //if (DataType == Driver.DataType.DataBlock)
            //{
            //    //Получение фрагмента адреса с номером ДБ 
            //    var dbn_ar = address.Split('.');

            //    string dbn_0 = dbn_ar[0];
            //    temporyAddress = dbn_ar[1];

            //    //Удаление лишнего текста
            //    dbn_0 = dbn_0.Replace("DB", "");

            //    //Удаление пробелов
            //    dbn_0 = dbn_0.Replace(" ", "");

            //    //Преобразование в численный вид
            //    DBNumber = Convert.ToInt16(dbn_0);
            //}

            //Получение начального байта в адресе переменной
            string stb = string.Empty;

            foreach (char symbol in temporyAddress.ToCharArray())
            {
                if (char.IsDigit(symbol) == true)
                {
                    stb += symbol;
                }

                //Если дошли до точки выходим из цикла
                if (symbol == '.')
                {
                    break;
                }
            }

            StartByteAddress = int.Parse(stb);
        }

        /// <summary>
        /// Метод для записи состояния переменной
        /// </summary>
        /// <param name="Value"></param>
        public virtual void Write(object Value)
        {
   
        }

        /// <summary>
        /// Метод для перевода типа к строковому виду
        /// </summary>
        public virtual void ObjectToString()
        {

        }

        /// <summary>
        /// Метод для получения Status
        /// </summary>
        public virtual void BuildStatus(Byte [] bytes, int offset)
        {

        }

        /// <summary>
        /// Метод для чтения тэга
        /// </summary>
        public virtual void Read()
        {

        }

        /// <summary>
        /// Метод для получения массива байт
        /// из телеграммы
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public byte[] GetBytes(byte[] bytes, int _offset)
        {
            List<byte> bts = new List<byte>();

            var offset = Math.Abs(_offset);

            //Если данных нет, тогда
            //шлем пустой массив
            if (bytes.Length < _offset + Lenght)
            {
                return new byte[1];
            }

            for (int i = offset; i < offset + Lenght; i++)
            {
                bts.Add(bytes[i]);
            }

            return bts.ToArray();
        }

        /// <summary>
        /// Метод для получения массива байт
        /// из телеграммы
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startindex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] GetBytes(byte[] bytes, int startindex, int count)
        {
            List<byte> bts = new List<byte>();

            var index = Math.Abs(startindex);

            //Если данных нет, тогда
            //шлем пустой массив
            if (bytes.Length < startindex + count)
            {
                return new byte[count];
            }

            for (int i = index; i < index + count; i++)
            {
                bts.Add(bytes[i]);
            }

            return bts.ToArray();
        }

        /// <summary>
        /// Получение числа uint из байт
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public uint ToUint(byte[] bytes)
        {
            // Обработка ошибок
            if (bytes.Length <  4)
            {
                return 0;
            }

            //Преобразование
            return bytes[3] | (uint)(bytes[2] << 8) | (uint)(bytes[1] << 16) | (uint)(bytes[0] << 24);
        }

        /// <summary>
        /// Получение числа uint из байт
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="statrindex"></param>
        /// <returns></returns>
        public uint ToUint(byte[] bytes, int statrindex)
        {
            //Обработка ошибок
            if (bytes.Length < statrindex + 4)
            {
                return 0;
            }

            //Преобразование
            var bts = GetBytes(bytes, statrindex, 4);
            return bts[3] | (uint)(bts[2] << 8) | (uint)(bts[1] << 16) | (uint)(bts[0] << 24);
        }



        #region Реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }

        }
        #endregion
    }
}
