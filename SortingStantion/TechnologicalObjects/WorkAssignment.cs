using S7Communication;
using System.ComponentModel;

namespace SortingStantion.TechnologicalObjects
{
    /// <summary>
    /// Класс, описывающий рабочее задание
    /// </summary>
    public class WorkAssignment : INotifyPropertyChanged
    {
        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticServer server
        {
            get
            {
                return DataBridge.server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Указатель на группу, где хранятся все тэгиК
        /// </summary>
        SimaticGroup group
        {
            get
            {
                return device.Groups[0];
            }
        }

        /// <summary>
        /// Тэг, отвечающий за Принять - завершить задание
        /// </summary>
        S7BOOL inWorkTag
        {
            get;
            set;
        }

        /// <summary>
        /// Флаг возвращает или задает
        /// принято ли задание
        /// </summary>
        public bool InWork
        {
            get
            {
                //Если тип не bool возвращаем false
                if (inWorkTag.Status is bool == false)
                {
                    return false;
                }

                //Возвращение значения тэга
                return (bool)inWorkTag.Status;
            }
            set
            {
                inWorkTag.Write(value);
            }
        }



        public WorkAssignment()
        {
            //Инициализация тэга
            inWorkTag = new S7BOOL("", "DB1.DBX182.0", group);
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
