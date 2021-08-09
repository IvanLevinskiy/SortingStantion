using Simulator.Controls;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Simulator
{
    /// <summary>
    /// Мост данных
    /// </summary>
    public static class DataBridge
    {
        /// <summary>
        /// Канвас
        /// </summary>
        public static Canvas canvas;

        /// <summary>
        /// Фотодатчик 1
        /// </summary>
        public static PhotoSensor FS1;

        /// <summary>
        /// Фотодатчик 2
        /// </summary>
        public static PhotoSensor FS2;

        /// <summary>
        /// Фотодатчик 3
        /// </summary>
        public static PhotoSensor FS3;

        /// <summary>
        /// Главный таймер
        /// </summary>
        public static DispatcherTimer timer;

        /// <summary>
        /// Метод для инициализации таймера
        /// </summary>
        public static void timerInit()
        { 
            if(timer == null)
            {
                timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
                timer.Start();
            }
        }

    }
}
