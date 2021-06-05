﻿using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Utilites;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace SortingStantion.TechnologicalObjects
{
    /// <summary>
    /// Класс, реализующий модель
    /// конвейера
    /// </summary>
    public class Conveyor : INotifyPropertyChanged
    {

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticServer server
        {
            get
            {
                return DataBridge.server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        public SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Указатель на группу, где хранятся все тэгиК
        /// </summary>
        public SimaticGroup group
        {
            get
            {
                return device.Groups[0];
            }
        }

        /// <summary>
        /// Команда ПУСК-ОСТАНОВКА
        /// </summary>
        public S7BOOL Run
        {
            get;
            set;
        }

        /// <summary>
        /// Флаг, указывающий, что
        /// линия запущена (для View Model)
        /// </summary>
        public bool LineIsRun
        {
            get 
            {
                if (Run.Status is bool? == false)
                {
                    return false;
                }

                return (bool)Run.Status == true;
            }
        }

        /// <summary>
        /// Флаг, указывающий, что
        /// линия остановлена (для View Model)
        /// </summary>
        public bool LineIsStop
        {
            get
            {
                if (Run.Status is bool? == false)
                {
                    return false;
                }

                return (bool)Run.Status == false;
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Conveyor()
        {
            //Тэг для запуска - останова линии
            Run = new S7BOOL("", "DB1.DBX132.0", group);

            //Подпись на событие по изминеию статуса работы
            //линии
            Run.ChangeValue += Run_ChangeValue;
        }

        /// <summary>
        /// Команда для запуска
        /// линии
        /// </summary>
        public ICommand StartLineCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //Если статус не является
                    //булевым значением - игнорируем обработку
                    //команды
                    if (Run.Status is bool? == false)
                    {
                        return;
                    }

                    //Если конвейер остановлен, запускаем его
                    if ((bool?)Run.Status == false)
                    {
                        //Если задание принято, то запускаем линию
                        //иначе уведомляем оператора сообщением
                        if (DataBridge.WorkAssignment.InWork == false)
                        {
                            customMessageBox mb = new customMessageBox("Ошибка", "Запуск невозможен, задание не принято в работу");
                            mb.Owner = DataBridge.MainScreen;
                            mb.ShowDialog();

                            return;
                        }

                        //Запись статуса в ПЛК
                        Run.Write(true);
                        return;
                    }
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для остановки
        /// линии
        /// </summary>
        public ICommand StopLineCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //Если статус не является
                    //булевым значением - игнорируем обработку
                    //команды
                    if (Run.Status is bool? == false)
                    {
                        return;
                    }

                    //Если конвейер запущен - останавливаем его
                    if ((bool?)Run.Status == true)
                    {
                        //Запись статуса в ПЛК
                        Run.Write(false);
                        return;
                    }

                    
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Метод, вызываемый при изминении
        /// статуса работы линии
        /// </summary>
        /// <param name="obj"></param>
        private void Run_ChangeValue(object value)
        {
            //Защита от неверных типов данных
            if (value is bool? == false)
            {
                return;
            }

            //Получение статуса работы конвейера
            bool _value = (bool)value;

            //Если конвейер запущен
            if (_value == true)
            {
                Action action = () =>
                {

                    //Вывод сообщения в окно информации
                    MSG msg = new MSG("Линия запущена", MSGTYPE.SUCCES);
                    DataBridge.MSGBOX.Add(msg);

                    
                };
                DataBridge.UIDispatcher.Invoke(action);
            }

            //Если конвейер остановлен
            if (_value == false)
            {
                Action action = () =>
                {
                    //Вывод сообщения в окно информации
                    MSG msg = new MSG("Линия остановлена", MSGTYPE.WARNING);
                    DataBridge.MSGBOX.Add(msg);
                };
                DataBridge.UIDispatcher.Invoke(action);
            }

            //уведомление модели представления
            OnPropertyChanged("LineIsRun");
            OnPropertyChanged("LineIsStop");
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
