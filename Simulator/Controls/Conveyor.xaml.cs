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
        DispatcherTimer timer;

        Random r;

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
            r = new Random();

            //Создание моделей боксов с рандомным периодом
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,0,0,3);
            timer.Tick += (e, s) =>
            {
                Box box = new Box(canvas);
                var random = r.Next(5, 10);
                timer.Interval = new TimeSpan(0, 0, 0, random);
            };

            this.Loaded += Conveyor_Loaded;           
           
        }

        private void Conveyor_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DataBridge.LineIsRun.ChangeValue += (o, n) =>
            {
                if (o is bool == false)
                {
                    return;
                }

                var lineisstart = (bool)o == false && (bool)n == true;
                var lineisstop =  (bool)o == true && (bool)n == false;
                

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
