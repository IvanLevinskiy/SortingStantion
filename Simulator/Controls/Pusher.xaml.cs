using S7Communication;
using System;
using System.Windows.Controls;

namespace Simulator.Controls
{
    /// <summary>
    /// Логика взаимодействия для Pusher.xaml
    /// </summary>
    public partial class Pusher : UserControl
    {
        /// <summary>
        /// Тэг толкателя
        /// </summary>
        public S7_Boolean Out;

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


        public Pusher()
        {
            InitializeComponent();

            DataBridge.Pusher = this;

            Out = (S7_Boolean)DataBridge.Device.GetTagByAddress("Q0.2");
            Out.ChangeValue += Out_ChangeValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Out_ChangeValue(object arg1, object arg2)
        {
            //Если новое значение не bool, выходим
            if (arg2 is bool == false)
            {
                return;
            }

            //Получаем значение свойства
            var value = (bool)arg2;

            //Если новое значение True, активируем
            if (value == true)
            {
                Activate();
                return;
            }

            //Если новое значение False, деактивируем
            if (value == false)
            {
                DeActivate();
                return;
            }

        }



        /// <summary>
        /// Метод для активации толкателя
        /// </summary>
        public void Activate()
        {
            Action action = () =>
            {
                //Анимация выдвижения толкателя
                var heght = this.ActualHeight;
                Border.Height = heght;

                //Поиск короба над толкателем
                var box = FindBox();

                //Если короб имеется
                //тогда его останавливаем
                //и поднимаем вверх
                if (box != null)
                {
                    box.Defect();
                }

            };
            this.Dispatcher.Invoke(action);
        }

        /// <summary>
        /// Метод для поиска короба, расположенного
        /// над толкателем
        /// </summary>
        /// <returns></returns>
        Box FindBox()
        {
            //получение списка коробов,
            //находящихсмя на ленте
            var boxs = Box.GetBoxs();

            var strtpos = Canvas.GetLeft(this);
            var endpos = strtpos + this.ActualWidth;

            //Поиск короба, находящегося
            //над отбраковщиком
            foreach (var currentbox in boxs)
            {
                var con_1 = StartPosition > currentbox.StartPosition && StartPosition < currentbox.EndPosition;
                var con_2 = EndPosition > currentbox.StartPosition && EndPosition < currentbox.EndPosition;

                if (con_1 || con_2)
                {
                    return currentbox;
                }
            }

            return null;
        }

        /// <summary>
        /// Метод для деактивации толкателя
        /// </summary>
        public void DeActivate()
        {
            Action action = () =>
            {
                Border.Height = 50;
            };
            this.Dispatcher.Invoke(action);
        }
    }
}
