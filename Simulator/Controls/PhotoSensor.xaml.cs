using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simulator.Controls
{
    /// <summary>
    /// Логика взаимодействия для PhotoSensor.xaml
    /// </summary>
    public partial class PhotoSensor : UserControl
    {
        /// <summary>
        /// Положение луча на канвасе
        /// </summary>
        public double myPosition;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public PhotoSensor()
        {
            InitializeComponent();

            Loaded += (e, s) =>
            {
                var canvas = DataBridge.canvas;
                var lpos = Canvas.GetLeft(this);
                var width = this.ActualWidth;
                myPosition = lpos + width / 2;
            };


            DataBridge.timerInit();
            DataBridge.timer.Tick += Timer_Tick;


        }

        /// <summary>
        /// Тик таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, System.EventArgs e)
        {
            var boxs = GetBoxs();

            var state = false;

            foreach (var box in boxs)
            {
                if (box.GetPhotoSensorState(myPosition) == true)
                {
                    state = true;
                }
            }

            if (state == true)
            {
                Rama.Fill = Brushes.Green;
            }
            else
            {
                Rama.Fill = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));
            }

        }

        Box [] GetBoxs()
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
