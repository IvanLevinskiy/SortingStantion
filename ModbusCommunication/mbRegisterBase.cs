using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Communication
{
    public class mbRegisterBase : INotifyPropertyChanged
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
        public int Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
                OnPropertyChanged("Address");
            }
        }
        int address;

        /// <summary>
        /// Статус регистра (string)
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
        /// Формат значения
        /// </summary>
        public string Format
        {
            get
            {
                return "{0:" + _format + "}";
            }
            set
            {
                _format = value;
                OnPropertyChanged("Format");
            }
        }
        protected string _format = null;

        /// <summary>
        /// Коэффициент (множитель)
        /// </summary>
        public double Factor
        {
            get
            {
                return factor;
            }
            set
            {
                factor = value;
                OnPropertyChanged("Factor");
            }
        }
        double factor = 1;

        /// <summary>
        /// Максимально допустимая величина
        /// значения
        /// </summary>
        public double HightValue;

        /// <summary>
        /// Минимально допустимая величина
        /// значения
        /// </summary>
        public double LowValue;

        /// <summary>
        /// Свойство, указывающее 
        /// на валидность данных
        /// </summary>
        public bool IsValid
        {
            get
            {
                return isValid;
            }
            set
            {
                //Проверка на появление валидности
                if (isValid == false && value == true) GotValid?.Invoke();

                //Проверка на утрату валидности
                if (isValid == true && value == false) LostValid?.Invoke();

                isValid = value;
                OnPropertyChanged("IsValid");
            }
        }
        bool isValid = false;

        /// <summary>
        /// Событие, возникающее при
        /// потери регистром валидности данных
        /// </summary>
        public event Action LostValid;


        /// <summary>
        /// Событие, возникающее при
        /// получении регистром валидности данных
        /// </summary>
        public event Action GotValid;

        /// <summary>
        /// Свойство для MVVM, указывающее
        /// на то, развернут ли узел
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        bool _isExpanded = false;

        /// <summary>
        /// Событие, генерируемое при
        /// изменении значения переменной
        /// </summary>
        public event Action<object> ChangeValue;

        /// <summary>
        /// Флаг, указывающий на необходимость 
        /// чтения регистра
        /// </summary>
        public bool ReadRequest = false;

        /// <summary>
        /// Флаг, указывающий на необходимость 
        /// записи значения в регистр
        /// </summary>
        public bool WriteRequest = false;

        /// <summary>
        /// Статус регистра
        /// </summary>
        public object Status
        {
            get
            {
                return status;
            }
            set
            {
                if (status.Equals(value) == false)
                {
                    status = value;
                    UpdatedValue(value);
                    ChangeValue?.Invoke(status);
                }

            }
        }
        object status = 0;

        /// <summary>
        /// Свойство - описание регистра
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
        string description = string.Empty;

        /// <summary>
        /// Указатель на родительскую группу
        /// </summary>
        public mbGroup Group;

        /// <summary>
        /// Тип данных
        /// </summary>
        public Types DataType
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
        /// Длина регистра в байтах
        /// </summary>
        public int Lenght
        {
            get;
            set;
        }

        /// <summary>
        /// Предыдущий статус переменной
        /// </summary>
        public object OldStatus = null;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbRegisterBase()
        { 
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
        public virtual bool Write(double Value)
        {
            return false;
        }

        /// <summary>
        /// Метод для перевода типа к строковому виду
        /// </summary>
        public virtual void UpdatedValue(object value)
        {

        }

        /// <summary>
        /// Метод для получения Status
        /// </summary>
        public virtual void BuildStatus(List<byte> bytes)
        {

        }

        /// <summary>
        /// Метод для чтения регистра
        /// </summary>
        public virtual void Read()
        {

        }

        /// <summary>
        /// преобразование значения 
        /// с инверсным следованием байт
        /// (для совместимости с типом Simatic)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal UInt16 ToSimaticType(uint value)
        {
            //Смена типа порядка байт
            var bts = BitConverter.GetBytes(value);

            //Создаем новый массив
            byte[] newbytes = new byte[2];
            newbytes[0] = bts[1];
            newbytes[1] = bts[0];

            //Возвращаем новое слово
            return BitConverter.ToUInt16(newbytes, 0);
        }

        /// <summary>
        /// Получение адреса последнего
        /// регистра
        /// </summary>
        /// <returns></returns>
        public int GetEndAddress()
        {
            //Получаем эквивалентное 
            //количество регистров
            var regcount = GetEcvivalentRegistersCount();
            return Address + regcount - 1;
        }

        /// <summary>
        /// Метод для получения эквивалентного
        /// количества регистров
        /// </summary>
        /// <returns></returns>
        public int GetEcvivalentRegistersCount()
        {
            return Lenght / 2;
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
