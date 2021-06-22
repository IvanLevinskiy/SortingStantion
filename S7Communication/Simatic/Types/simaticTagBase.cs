using S7Communication.Utilites;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace S7Communication
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
        /// Полное имя тэга
        /// </summary>
        public string FullName
        {
            get
            {
                return $"{group.FullName}.{Name}";
            }
        }

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
                
                //Разбор адреса
                ParseAddress(address);

                //Обновление модели представления
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
                //Проверка на нулевой указатель
                if (status == null)
                {
                    //status = value;
                    //UpdatedValue(value);
                    return;
                }

                //Обработка полученых данных
                if (status != value)
                {
                    //Сохранение старого статуса
                    var oldstatus = status;

                    //Сохранение нового статуса
                    status = value;
                    UpdatedValue(value);

                    //Извещение подписчиков
                    ChangeValue?.Invoke(oldstatus, value);


                    OnPropertyChanged("Status");
                }  
            }
        }
        object status = 0;

        /// <summary>
        /// Событие, генерируемое при
        /// изменении значения переменной
        /// </summary>
        public event Action<object, object> ChangeValue;

        /// <summary>
        /// Свойство для MVVM
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");

                //Костыль
                group.ParentDevice.server.FindeSelectedElement();
            }
        }
        bool _isSelected;

        /// <summary>
        /// Тип данных ПЛК
        /// </summary>
        public MemmoryArea DataType
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
        /// Родительское устройство
        /// </summary>
        public SimaticDevice device
        {
            get
            {
                return group.ParentDevice;
            }
        }

        /// <summary>
        /// Родительская группа
        /// </summary>
        public SimaticGroup group
        {
            get;
            set;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public simaticTagBase()
        {
            
        }

        /// <summary>
        /// Метод для разбора адреса
        /// </summary>
        public void ParseAddress(string address)
        {
            //Получение типа данных
            DataType = Converters.StringToDataType(address);

            //Получение номера блока данных
            if (DataType == MemmoryArea.DataBlock)
            {
                //Получение фрагмента адреса с номером ДБ 
                var dbn_ar = address.Split('.');

                string dbn_0 = dbn_ar[0];
                temporyAddress = dbn_ar[1];

                //Удаление лишнего текста
                dbn_0 = dbn_0.Replace("DB", "");

                //Удаление пробелов
                dbn_0 = dbn_0.Replace(" ", "");

                //Преобразование в численный вид
                DBNumber = Convert.ToInt16(dbn_0);
            }

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
        public virtual bool Write(object Value)
        {
            return false;
        }

        /// <summary>
        /// Метод для записи состояния переменной
        /// </summary>
        /// <param name="Value"></param>
        public virtual bool Write(string Value)
        {
            return false;
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
        /// Метод для перевода типа к строковому виду
        /// </summary>
        public virtual void UpdatedValue(object value)
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
