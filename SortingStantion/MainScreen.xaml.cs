using System;
using System.Windows;

namespace SortingStantion.MainScreen
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainScreen : Window
    {
        public MainScreen()
        {
            InitializeComponent();

            //Sharp7.S7Client s7Client = new Sharp7.S7Client();
            //var result = s7Client.ConnectTo("192.168.3.70", 0, 1);
            //var array = new byte[2274];
            //result = s7Client.DBRead(5, 0, 2274, array);
            //var ec = s7Client.ErrorText(result);

            //int dt = 0;

            //result = s7Client.PlcGetStatus(ref dt);
            //ec = s7Client.ErrorText(result);


            //Внесение в базу данных сообщения о запуске приложения 
            DataBridge.AlarmLogging.AddMessage("Запуск приложения", Models.MessageType.Event);

            //ПЕредача экземпляра окна
            DataBridge.MainScreen = this;

            //Передача  UI Dispatcher
            DataBridge.UIDispatcher = this.Dispatcher;

            //Инициализируем свойство IsAvailable ПЛК
            //для того, чтоб отображалась ошибка
            DataBridge.S7Server.Devices[0].IsAvailable = false;

            this.Closing += MainScreen_Closing;
        }

        /// <summary>
        /// Метод для остановки конвейера при 
        /// завершении работы программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainScreen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Останоака ленты
            DataBridge.Conveyor.Stop();

            //Запись файла с результатом
            //операций (если отчет не отправлен)
            DataBridge.Report.Save();
        }

        /// <summary>
        /// Метод для закрытия окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch
            { 
            
            }
        }
    }
}
