using System;
using System.Windows.Controls;

namespace Simulator.Controls
{
    /// <summary>
    /// Логика взаимодействия для Box.xaml
    /// </summary>
    public partial class Box : UserControl
    {
        Canvas canvas;

        public Box()
        {
            InitializeComponent();
        }

        public Box(Canvas canvas)
        {
            InitializeComponent();
            this.canvas = canvas;

            canvas.Children.Add(this);

            this.Height = 100;
            this.Width = 100;

            Canvas.SetTop(this, 30);
            Canvas.SetLeft(this, 0);

            DataBridge.timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var position = Canvas.GetLeft(this) + 1;
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

        public bool GetPhotoSensorState(double FSPostion)
        {
            var startpos = Canvas.GetLeft(this);
            var endpos = startpos + this.ActualHeight;

            return startpos <= FSPostion && FSPostion <= endpos;
        }
    }
}
