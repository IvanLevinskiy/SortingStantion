using S7Communication;
using SortingStantion.Utilites;
using System.ComponentModel;
using System.Windows.Input;

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


        /// <summary>
        /// Конструктор класса
        /// </summary>
        public WorkAssignment()
        {
            //Инициализация тэга
            inWorkTag = new S7BOOL("", "DB1.DBX182.0", group);
        }

        /// <summary>
        /// Команда - принять задание
        /// </summary>
        public ICommand AcceptTaskCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {

                    if (InWork == false)
                    {
                        //Запись статуса в ПЛК
                        inWorkTag.Write(true);
                        return;
                    }
                },
                (obj) => (InWork == false));
            }
        }

        /// <summary>
        /// Команда для завершения задания
        /// </summary>
        public ICommand FinishTaskCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {

                    if (InWork == true)
                    {
                        //Запись статуса в ПЛК
                        inWorkTag.Write(false);
                        return;
                    }
                },
                (obj) => (InWork == true));
            }
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
