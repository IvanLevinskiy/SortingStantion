using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Simulator.Controls
{
    /// <summary>
    /// Логика взаимодействия для Conveyor.xaml
    /// </summary>
    public partial class Conveyor : UserControl
    {
        public Conveyor()
        {
            //Инициализация UI
            InitializeComponent();

            //Передача канвы
            DataBridge.canvas = canvas;

            //Передача указателей на фотодатчики
            DataBridge.FS1 = fs1;
            DataBridge.FS2 = fs2;
            DataBridge.FS3 = fs3;


            //Объявление рандома
            Random r = new Random();

            //Создание моделей боксов с рандомным периодом
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,0,0,3);
            timer.Start();
            timer.Tick += (e, s) =>
            {
                Box box = new Box(canvas);
                var random = r.Next(3, 6);
                timer.Interval = new TimeSpan(0, 0, 0, random);
            };
           
        }
    }
}
