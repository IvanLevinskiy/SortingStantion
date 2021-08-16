using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Simulator.Controls
{
    /// <summary>
    /// Логика взаимодействия для Box.xaml
    /// </summary>
    public partial class Box : UserControl
    {
        Canvas canvas;

        /// <summary>
        /// Позиция, с которой начинается короб
        /// </summary>
        public double StartPosition
        {
            get
            {
                return Canvas.GetLeft(this);
            }
        }

        /// <summary>
        /// Позиция, на которой заканчивается короб
        /// </summary>
        public double EndPosition
        {
            get
            {
                return Canvas.GetLeft(this) + this.ActualWidth;
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Box()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="canvas"></param>
        public Box(Canvas canvas)
        {
            InitializeComponent();
            this.canvas = canvas;

            canvas.Children.Add(this);

            this.Height = 150;
            this.Width = 50;

            Canvas.SetTop(this, -30);
            Canvas.SetLeft(this, 0);

            DataBridge.timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var position = Canvas.GetLeft(this) + 3;
            Canvas.SetLeft(this, position);
            Utilize(position);


        }

        void Utilize(double currentleftposition)
        {
            var width = canvas.ActualWidth;

            if (currentleftposition + this.Width >= width)
            {
                canvas.Children.Remove(this);
                DataBridge.timer.Tick -= Timer_Tick;
            }
        }

        /// <summary>
        /// Метод, вызываемый если короб отбраковывается
        /// </summary>
        public void Defect()
        {
            //Остановка короба
            DataBridge.timer.Tick -= Timer_Tick;

            //Получение позиции короба, на которую его надо
            //переместить при отбраковке
            var topPosition = Canvas.GetTop(DataBridge.Pusher);
            topPosition = topPosition - this.ActualHeight;

            //Установка позиции сверху
            Canvas.SetTop(this, topPosition);

            //Запуск таймера
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,0,0,1);
            timer.Start();

            //Удаление бокса
            timer.Tick += (s, e) =>
            {
                canvas.Children.Remove(this);
            };
        }

        public bool GetPhotoSensorState(double FSPostion)
        {
            var startpos = Canvas.GetLeft(this);
            var endpos = startpos + this.ActualWidth;

            return startpos <= FSPostion && FSPostion <= endpos;
        }

        /// <summary>
        /// Метод для получения списка коробов на ленте
        /// </summary>
        /// <returns></returns>
        public static Box[] GetBoxs()
        {
            List<Box> boxes = new List<Box>();

            foreach (var element in DataBridge.canvas.Children)
            {
                if (element is Box)
                {
                    boxes.Add((Box)element);
                }
            }

            return boxes.ToArray();
        }
    }
}
