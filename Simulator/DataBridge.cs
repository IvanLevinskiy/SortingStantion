using S7Communication;
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
        /// Simatic Server
        /// </summary>
        public static SimaticClient Server;

        /// <summary>
        /// Указатель на отбраковщик
        /// </summary>
        public static Pusher Pusher;

        /// <summary>
        /// Simatic Device
        /// </summary>
        public static SimaticDevice Device
        {
            get 
            {
                if (device == null)
                {
                    S7Init();
                }

                return device;
            }
            set
            {
                device = value;
            }
        }
        public static SimaticDevice device;

        /// <summary>
        /// Тэг, что линия работает
        /// </summary>
        public static S7_Boolean LineIsRun;

        /// <summary>
        /// Инициализация S7
        /// </summary>
        public static void S7Init()
        {
            if (device == null)
            {
                //Создание сервера
                DataBridge.Server = new SimaticClient();
                DataBridge.Device = new SimaticDevice("192.168.3.70", S7Communication.Enumerations.CpuType.S71200, 0, 1);
                DataBridge.Device.AddGroup(new SimaticGroup());
                DataBridge.Server.AddDevice(DataBridge.Device);

                LineIsRun = (S7_Boolean)DataBridge.Device.GetTagByAddress("DB15.DBX2.6");
            }
        }

        /// <summary>
        /// Главный таймер
        /// </summary>
        public static DispatcherTimer timer;

        /// <summary>
        /// Метод для инициализации таймера
        /// </summary>
        public static void timerInit()
        {
            var device = Device;

            if(timer == null)
            {
                timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 1);


                DataBridge.LineIsRun.ChangeValue += (o, n) =>
                {
                    if (o is bool == false)
                    {
                        return;
                    }

                    var lineisstart = (bool)o == false && (bool)n == true;
                    var lineisstop = (bool)o == true && (bool)n == false;


                    if (lineisstart == true)
                    {
                        timer.Start();
                        return;
                    }


                    if (lineisstop == true)
                    {
                        timer.Stop();
                        return;
                    }
                };
            }
        }

    }
}
