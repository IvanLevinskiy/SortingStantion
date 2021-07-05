using SortingStantion.Models;
using System;
using System.IO.Ports;
using System.Windows.Controls;

namespace SortingStantion.Controls
{
    /// <summary>
    /// Логика взаимодействия для ComboBoxSerialPorts.xaml
    /// </summary>
    public partial class ComboBoxSerialPorts : UserControl
    {
        Setting setting;

        public ComboBoxSerialPorts()
        {
            InitializeComponent();
            setting = DataBridge.SettingsFile.GetSetting("SerialPort232Name");

            //Загрузка портов
            var ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                var portname = ports[i];
                combobox.Items.Add(ports[i]);

                if (portname == ports[i])
                {
                    combobox.SelectedIndex = i;
                }
            }

            //Подпись на события
            combobox.DropDownOpened += ComboBox_DropDownOpened;
            combobox.SelectionChanged += combobox_SelectionChanged;
        }

        /// <summary>
        /// Метод, вызываемый при открытии
        /// комбобокса. При открытии происходит
        /// 
        /// загрузка доступных портов в список
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var ports = SerialPort.GetPortNames();
            combobox.Items.Clear();
            foreach (var port in ports)
            {
                combobox.Items.Add(port);
            }
        }

        private void combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combobox.SelectedItem == null)
            {
                return;
            }

            //Сохранение новой настройки в файл конфигурации
            var newvalue = combobox.SelectedItem.ToString();
            setting.SetValue(newvalue);

            //Смена имени порта, к которому подключен
            //ручной сканер
            DataBridge.Scaner.Load(newvalue);
        }
    }
}
