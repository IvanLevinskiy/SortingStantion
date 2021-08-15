using S7Communication;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simulator.Controls
{
    /// <summary>
    /// Логика взаимодействия для PhotoSensor.xaml
    /// </summary>
    public partial class PhotoSensor : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Надпись в фотодатчике
        /// </summary>
        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                caption = value;
                OnPropertyChanged("Caption");

            }
        }
        string caption = "FS ***";

        S7_Boolean tag;

        /// <summary>
        /// Операнд в памяти ПЛК
        /// </summary>
        public string S7Operand
        {
            set
            {
                tag = (S7_Boolean)DataBridge.Device.GetTagByAddress(value);
            }
        }

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

            DataContext = this;

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
                tag.Write(true);
            }
            else
            {
                Rama.Fill = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));
                tag.Write(false);
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
