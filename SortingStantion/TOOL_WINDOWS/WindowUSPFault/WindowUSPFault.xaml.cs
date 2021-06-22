﻿using System;
using System.Windows;
using System.Windows.Threading;

namespace SortingStantion.frameUSPFault
{
    /// <summary>
    /// Логика взаимодействия для frameUSPFault.xaml
    /// </summary>
    public partial class frameUSPFault : Window
    {
        /// <summary>
        /// Счетчик обратного отсчета
        /// </summary>
        int time = 60;

        /// <summary>
        /// Таймер обратного отсчета
        /// </summary>
        DispatcherTimer timer;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public frameUSPFault()
        {
            InitializeComponent();

            //Подписка на событие
            //возникаемого при закрытии окна
            this.Closing += Window_Closing;

            //Инициализация таймера
            timer = new DispatcherTimer();
            
            //Инициализация времени тика
            timer.Interval = new TimeSpan(0,0,0,1);
            
            //Инициализация делегата
            timer.Tick += (s, e) =>
            {
                time--;

                //Если счетчик досчитал,
                //выключаем комплекс
                if (time < 0)
                {
                    App.Current.Shutdown();
                    return;
                }

                txMessage.Text = $"Потеря питания! Комплекс будет отключен через {time} секунд";

            };

            //Запуск таймера
            timer.Start();

            //Получение тэга, ошибки USP
            var tag = DataBridge.S7Server.Devices[0].GetTagByAddress("DB6.DBX13.4");

            //При появлении питания закрытия окна
            tag.ChangeValue += (oldstate, newstate) =>
            {

                if ((bool?)newstate == false)
                {
                    Action action = () =>
                    {
                        //Отписка от события
                        //возникаемого при закрытии окна
                        this.Closing -= Window_Closing;

                        //Остановка таймера 
                        //обратного отсчета
                        timer.Stop();

                        //Закрытие окна
                        this.Close();
                    };
                    DataBridge.UIDispatcher.Invoke(action);
                }
                
            };
        }

        /// <summary>
        /// Метод, вызываемый при закрытии окна
        /// (для отмены закрытия окна)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
