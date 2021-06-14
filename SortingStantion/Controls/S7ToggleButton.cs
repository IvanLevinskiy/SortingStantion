﻿using S7Communication;
using SortingStantion.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;

namespace SortingStantion.Controls
{
    public class S7ToggleButton : Button, INotifyPropertyChanged
    {
        /// <summary>
        /// Состояние контролла
        /// </summary>
        public bool? IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
        bool? isChecked = false;

        /// <summary>
        /// Минимальный уровень доступа
        /// </summary>
        public int MinAccesLevel
        {
            get
            {
                return minAccesLevel;
            }
            set
            {
                minAccesLevel = value;
                this.IsEnabled = CheckAccesLevel();
            }
        }
        int minAccesLevel = 1;

        /// <summary>
        /// Адрес регистра
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

                S7TAG = (S7BOOL)DataBridge.S7Server.Devices[0].GetTagByAddress(address);

                //Если регистр не найден
                if (S7TAG == null)
                {
                    return;
                }

                //Подписка на изменение значение тэга
                S7TAG.ChangeValue += S7TAG_ChangeValue;

                //Подписка на возобновление соединения с ПЛК
                //S7TAG.device.GotConnection += Device_GotConnection;
            }
        }

        

        string address = "Не задан";

        /// <summary>
        /// Поле - связанный бит
        /// </summary>
        public S7BOOL S7TAG
        {
            get;
            set;
        }


        /// <summary>
        /// Конструктор
        /// </summary>
        public S7ToggleButton()
        {
            //Установка контекста данных
            DataContext = this;

            //Подписка на событие по клику
            this.Click += S7ToggleButton_Checked;

            //Подписка на изменение текущего пользователя
            //для установки доступа к контролу
            DataBridge.MainAccesLevelModel.ChangeUser += Users_ChangeUser;
        }

        /// <summary>
        /// Метод, вызываемый при смене пользователя
        /// </summary>
        /// <param name="obj"></param>
        private void Users_ChangeUser(int currentuser, User newuser)
        {
            this.IsEnabled = CheckAccesLevel();
        }

        /// <summary>
        /// Метод, вызываемый при попытке изменить
        /// состояние регистра
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void S7ToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            //Инвертируем значение, которое необходимо записать
            bool State = !(bool)IsChecked;

            //Если экзкмпляр бита не 
            //пустой объект
            if (S7TAG != null)
            {
                //Записываем значение с трех попыток
                for (int i = 0; i <= 3; i++)
                {
                    var result = S7TAG.Write(State);

                    //Технологическая пауза
                    Thread.Sleep(50);
                }
            }
        }

        /// <summary>
        /// Mеняем состояние toggleButton при изменении
        /// состояния бита обратной связи
        /// </summary>
        /// <param name="obj"></param>
        private void S7TAG_ChangeValue(object obj)
        {
            bool? value = (bool?)obj;

            Action action = () =>
            {
                this.Click -= S7ToggleButton_Checked;
                this.IsChecked = value;
                this.Click += S7ToggleButton_Checked;
            };
            this.Dispatcher.Invoke(action);
        }


        /// <summary>
        /// Метод для проверки уровня доступа
        /// </summary>
        /// <param name="accesslevel"></param>
        /// <returns></returns>
        bool CheckAccesLevel()
        {
            //Проверка регистра на валидность значения
            var isvalid = false;
            if (S7TAG != null)
            {
                isvalid = true;
            }

            //Разрешение управления при соответствующем уровне доступаи валидном состоянии регистра
            return ((int)DataBridge.MainAccesLevelModel.CurrentUser.AccesLevel >= MinAccesLevel) & isvalid;
        }

        /// <summary>
        /// Метод, вызываемый при изминении
        /// состояния подключения к целевому устройству
        /// </summary>
        /// <param name="obj"></param>
        private void ChangeValidState(bool ConnectionState)
        {
            //Ихменение разрешения управления контроллом при изминении состояния
            //подключения к целевому устройству
            Action action = () =>
            {
                this.IsEnabled = CheckAccesLevel() & ConnectionState;
            };
            this.Dispatcher.Invoke(action);
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
