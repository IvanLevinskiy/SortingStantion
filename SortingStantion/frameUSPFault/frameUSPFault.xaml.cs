using System;
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
            var tag = DataBridge.server.Devices[0].GetTagByAddress("DB6.DBX13.4");

            //При появлении питания закрытия окна
            tag.ChangeValue += (state) =>
            {

                if ((bool)state == false)
                {
                    Action action = () =>
                    {
                        timer.Stop();
                        this.Close();
                    };
                    DataBridge.UIDispatcher.Invoke(action);
                }
                
            };
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
