using S7Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simulator.Controls
{
    /// <summary>
    /// Логика взаимодействия для Matrix.xaml
    /// </summary>
    public partial class Matrix : UserControl
    {
        /// <summary>
        /// Тэг, что фаза сканирования
        /// </summary>
        public static S7_Boolean ScanerPhaseOn;

        /// <summary>
        /// Тэг, что фаза сканирования закончилась
        /// </summary>
        public static S7_Boolean ScanerPhaseOff;

        public Matrix()
        {
            InitializeComponent();

            ScanerPhaseOn = (S7_Boolean)DataBridge.Device.GetTagByAddress("DB15.DBX2.0");
            ScanerPhaseOff = (S7_Boolean)DataBridge.Device.GetTagByAddress("DB15.DBX2.1");

            ScanerPhaseOn.ChangeValue += ScanerPhaseOn_ChangeValue;
            ScanerPhaseOff.ChangeValue += ScanerPhaseOff_ChangeValue;
        }

        private void ScanerPhaseOn_ChangeValue(object arg1, object arg2)
        {
            Action action = () =>
            {
                var value = (bool)arg2;

                if (value == true)
                {
                    laser.Visibility = Visibility.Visible;
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
                    return;
                }
            };

            this.Dispatcher.Invoke(action);

        }

        
    }
}
