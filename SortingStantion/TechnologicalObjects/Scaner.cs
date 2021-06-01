using SortingStantion.Models;
using System;
using System.IO.Ports;

namespace SortingStantion.TechnologicalObjects
{

    public class Scaner
    {
        /// <summary>
        /// Глобальный файл настроек
        /// </summary>
        SettingsFile sf
        {
            get 
            {
                return DataBridge.SettingsFile;
            }
        }

        /// <summary>
        /// Порт, к которому подключен сканер
        /// </summary>
        SerialPort port;

        /// <summary>
        /// Событие, генерируемое при получении
        /// новых данных сканером
        /// </summary>
        public event Action<string> NewDataNotification;
        

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Scaner()
        {
            Load();
        }

        /// <summary>
        /// Метод для загрузки настроек 
        /// порта
        /// </summary>
        public void Load()
        {
            //Если порт был открыт - закрываем его
            if (port != null)
            {
                port.Close();
            }

            //Инициализация порта, к которому
            //подключен сканер
            port = new SerialPort();
            port.PortName = sf.GetValue("SerialPort232Name");
            port.BaudRate = int.Parse(sf.GetValue("SerialPort232BaudRate"));
            port.DataBits = int.Parse(sf.GetValue("SerialPort232DataBits"));
            port.Parity = ToParity(sf.GetValue("SerialPort232StopBits"));
            port.StopBits = ToStopBits(sf.GetValue("SerialPort232StopBits"));
        }

        /// <summary>
        /// Метод для приведение строки
        /// к типу Parity
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Parity ToParity(string value)
        {

            if (value.ToUpper() == "ODD")
            {
                return Parity.Odd;
            }

            if (value.ToUpper() == "EVEN")
            {
                return Parity.Even;
            }

            if (value.ToUpper() == "MARK")
            {
                return Parity.Mark;
            }

            if (value.ToUpper() == "SPACE")
            {
                return Parity.Space;
            }

            return Parity.None;
        }

        /// <summary>
        /// Метод для приведение строки
        /// к типу StopBits
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        StopBits ToStopBits(string value)
        {
            if (value.ToUpper() == "NONE")
            {
                return StopBits.None;
            }

            if (value.ToUpper() == "ONEPOINTFIVE")
            {
                return StopBits.OnePointFive;
            }

            if (value.ToUpper() == "TWO")
            {
                return StopBits.Two;
            }

            return StopBits.One;
        }

        /// <summary>
        /// Метод для включения сканера в работу
        /// </summary>
        public void Start()
        {
            try
            {
                port.Open();
                port.DataReceived += Port_DataReceived;
            }
            catch(Exception ex)
            { 
            
            }
        }

        /// <summary>
        /// Событие, вызываемое при 
        /// получении новых данных от сканера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Чтение данных из строки
            var data = port.ReadLine();

            //Уведомление подписчиков о новых данных
            NewDataNotification?.Invoke(data);
        }

        /// <summary>
        /// Метод для завершения работыт сканера
        /// </summary>
        public void Stop()
        {

        }
    }
}
