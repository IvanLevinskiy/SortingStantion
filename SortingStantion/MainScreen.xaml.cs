using SortingStantion.Controls;
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

            //Уведомление подписчиков о завершении
            //загрузки приложения
            DataBridge.LoadCompleteNotification();
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
