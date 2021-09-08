using S7Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Simulator.Controls
{
    /// <summary>
    /// Логика взаимодействия для Matrix.xaml
    /// </summary>
    public partial class Matrix : UserControl
    {
        /// <summary>
        /// Сканер сканирует
        /// </summary>
        bool IsRun
        {
            get
            {
                return laser.Visibility == Visibility.Visible;
            }
        }

        /// <summary>
        /// Список кодов
        /// </summary>
        List<string> Codes = new List<string>();

        /// <summary>
        /// Тэг, что фаза сканирования
        /// </summary>
        public static S7_Boolean ScanerPhaseOn;

        /// <summary>
        /// Тэг, что фаза сканирования закончилась
        /// </summary>
        public static S7_Boolean ScanerPhaseOff;

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
        /// Триггер для сканирования
        /// </summary>
        bool ScanTrigger = false;

        /// <summary>
        /// Сигнал - GoodRead
        /// </summary>
        S7_Boolean GoodRead;

        /// <summary>
        /// Сигнал - GoodRead
        /// </summary>
        S7_Boolean NoRead;

        /// <summary>
        /// Штрихкод
        /// </summary>
        S7_CharsArray SCAN_DATA;

        public Matrix()
        {
            InitializeComponent();

            LoadBarcodes();

            GoodRead = (S7_Boolean)DataBridge.Device.GetTagByAddress("DB15.DBX4.3");
            NoRead = (S7_Boolean)DataBridge.Device.GetTagByAddress("DB15.DBX4.4");
            SCAN_DATA = (S7_CharsArray)DataBridge.Device.GetTagByAddress("DB9.DBD14-CHARS100");


            ScanerPhaseOn = (S7_Boolean)DataBridge.Device.GetTagByAddress("DB15.DBX2.0");
            ScanerPhaseOff = (S7_Boolean)DataBridge.Device.GetTagByAddress("DB15.DBX2.1");

            ScanerPhaseOn.ChangeValue += ScanerPhaseOn_ChangeValue;
            ScanerPhaseOff.ChangeValue += ScanerPhaseOff_ChangeValue;

            DataBridge.timer.Tick += Timer_Tick;
        }



        /// <summary>
        /// Метод для загрузки кодов 
        /// из файла
        /// </summary>
        void LoadBarcodes()
        {
            StreamReader sr = new StreamReader("codes.txt");
            var data = sr.ReadToEnd();
            sr.Close();

            var lines = data.Split('\n');
            foreach (var code in lines)
            {
                Codes.Add(code);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (IsRun == false)
            {
                return;
            }

            if (ScanTrigger == false)
            {
                return;
            }

            var box = FindBox();
            if(box == null)
            {
                return;
            }

            //Получаем штрихкод из файла
            string barcode = string.Empty;
            if(Codes.Count != 0)
            {
                barcode = Codes[0];
                Codes.RemoveAt(0);
            }

            ScanTrigger = false;

            //Если штрихкод пустой
            if (string.IsNullOrEmpty(barcode) == true)
            {
                Task.Factory.StartNew(() =>
                {
                    SCAN_DATA.Write("");
                    Thread.Sleep(1000);
                    NoRead.Write(true);
                    Thread.Sleep(1000);
                    NoRead.Write(false);
                });
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    SCAN_DATA.Write(barcode);
                    Thread.Sleep(1000);
                    GoodRead.Write(true);
                    Thread.Sleep(1000);
                    GoodRead.Write(false);
                });
            }

        }

        private void ScanerPhaseOn_ChangeValue(object arg1, object arg2)
        {
            Action action = () =>
            {
                var value = (bool)arg2;

                if (value == true)
                {
                    laser.Visibility = Visibility.Visible;
                    ScanTrigger = true;
                    return;
                }

                if (value == false)
                {
                    laser.Visibility = Visibility.Hidden;
                    return;
                }
            };
            this.Dispatcher.Invoke(action);

            
        }

        private void ScanerPhaseOff_ChangeValue(object arg1, object arg2)
        {
            Action action = () =>
            {
                var value = (bool)arg2;

                if (value == true)
                {
                    laser.Visibility = Visibility.Hidden;
                    ScanTrigger = false;
                    return;
                }
            };

            this.Dispatcher.Invoke(action);

        }

        /// <summary>
        /// Метод для поиска короба, расположенного
        /// над сканером
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

    }
}
