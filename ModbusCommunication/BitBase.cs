using System;
using System.ComponentModel;

namespace Communication
{
    public class BitBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Бит
        /// </summary>
        public byte Bit
        {
            get
            {
                return bit;
            }
            set
            {
                bit = value;
                OnPropertyChanged("Bit");
            }
        }
        byte bit = 0;

        /// <summary>
        /// Состояние бита
        /// </summary>
        public bool? State
        {
            get
            {
                return bitstate;
            }
            set
            {
                //Выделение положительного
                //фронта сигнала
                if (value == true && bitstate == false)
                {
                    FrontPositive?.Invoke();
                }

                //Выделение негативного
                //фронта сигнала
                if (value == false && bitstate == true)
                {
                    FrontNegative?.Invoke();
                }

                //Извещение подписчиков
                if (value != bitstate)
                {
                    ChangeValue?.Invoke(value);
                    RefreshValue(value);
                }

                bitstate = value;
                OnPropertyChanged("State");
            }
        }
        bool? bitstate = null;

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
                if (isValid == false && value == true)
                {
                    isValid = value;
                    GotValid?.Invoke();
                    ChangeValidState?.Invoke(value);
                    OnPropertyChanged("IsValid");
                    return;
                }

                //Проверка на утрату валидности
                if (isValid == true && value == false)
                {
                    isValid = value;
                    LostValid?.Invoke();
                    ChangeValidState?.Invoke(value);
                    OnPropertyChanged("IsValid");
                    return;
                } 
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
        /// Событие, возникающее при
        /// изминении состояния валидности данных
        /// </summary>
        public event Action<bool> ChangeValidState;

        /// <summary>
        /// Возникает при положительном фронте
        /// сигнала
        /// </summary>
        public event Action FrontPositive;

        /// <summary>
        /// Событие возникает при отрицательном фронте сигнала
        /// </summary>
        public event Action FrontNegative;

        /// <summary>
        /// Метод для записи состояния переменной
        /// </summary>
        /// <param name="Value"></param>
        public virtual void RefreshValue(bool? value )
        {

        }

        /// <summary>
        /// Метод для записи значения
        /// </summary>
        /// <param name="value"></param>
        public virtual bool Write(bool value)
        {
            return false;
        }

        /// <summary>
        /// Событие, генерируемое при ихменении
        /// состояния бита
        /// </summary>
        public event Action<bool?> ChangeValue;

        /// <summary>
        /// Указатель на слово (двойное слово),
        /// в котором хранится бит
        /// </summary>
        public mbRegisterBase Register
        {
            get
            {
                return register;
            }
            set
            {
                register = value;
            }
        }
        mbRegisterBase register;


        #region РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

    }
}
