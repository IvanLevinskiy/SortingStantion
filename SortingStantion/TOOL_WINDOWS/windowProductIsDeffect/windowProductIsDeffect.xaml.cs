using S7Communication;
using SortingStantion.Controls;
using System.Windows;

namespace SortingStantion.TOOL_WINDOWS.windowProductIsDeffect
{
    /// <summary>
    /// Логика взаимодействия для windowProductIsDeffect.xaml
    /// </summary>
    public partial class windowProductIsDeffect : Window
    {
       
        /// <summary>
        /// Указательн на сообщение
        /// в зоне информации, которое надо удалить
        /// </summary>
        UserMessage userMessage;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="serialnumberdeffect"></param>
        public windowProductIsDeffect(string serialnumberdeffect, UserMessage userMessage)
        {
            //Инициализация UI
            InitializeComponent();

            //Передача указателя на сообщение в зоне
            //информации, которое надо удалить при нажатии кнопки Отмена
            this.userMessage = userMessage;

            //Передача указателя на окно, в центе которого 
            //надо разместить окна
            this.Owner = DataBridge.MainScreen;

            //Формирование правиьного сообщения
            txMessage.Text = $"     Продукт {serialnumberdeffect} числится в браке. Найдите его ручным сканером и удалите с конвейера.";

            //Подписка на события
            this.Closing += Window_Closing;
        }

        /// <summary>
        /// Метод, вызываемый при клике по кнопке - ОТМЕНА
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DataBridge.MSGBOX.Remove(userMessage);
            this.Closing -= Window_Closing;
            this.Close();
        }

        /// <summary>
        /// Метод, вызываемый при закрытии окна
        /// (отмена закрытия окна)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
