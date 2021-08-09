using S7Communication;
using SortingStantion.S7Extension;
using System;
using System.Windows;

namespace SortingStantion.TOOL_WINDOWS.windowOverDeffectCounter
{
    /// <summary>
    /// Логика взаимодействия для windowOverDeffectCounter.xaml
    /// </summary>
    public partial class windowOverDeffectCounter : Window
    {
        /// <summary>
        /// Указательн на сообщение
        /// в зоне информации, которое надо удалить
        /// </summary>
        S7DiscreteAlarm alarm;

        /// <summary>
        /// Счетчик брака
        /// </summary>
        S7_DWord DEFFECT_PRODUCTS_COUNTER;

        public windowOverDeffectCounter(S7_DWord DEFFECT_PRODUCTS_COUNTER, S7DiscreteAlarm alarm)
        {
            //Инициализация UI
            InitializeComponent();

            //Передача экземпляра тэга со счетчиком ошибок
            this.DEFFECT_PRODUCTS_COUNTER = DEFFECT_PRODUCTS_COUNTER;

            //Передача указателя на сообщение в зоне
            //информации, которое надо удалить при нажатии кнопки Отмена
            this.alarm = alarm;

            //Передача указателя на окно, в центе которого 
            //надо разместить окна
            this.Owner = DataBridge.MainScreen;

            //Формирование правиьного сообщения
            txMessage.Text = $"     Конвейер остановлен после {DEFFECT_PRODUCTS_COUNTER.StatusText} продуктов, отбракованных подряд.";

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
            //Квитирование аварии
            alarm.Write(false);

            //Уменьшение счетчика подряд идущего брака на единицу
            var counter = (UInt32)DEFFECT_PRODUCTS_COUNTER.Status;
            counter = counter - 1;
            DEFFECT_PRODUCTS_COUNTER.Write(counter);

            //Отписка от метода, вызываемого при закрытии окна
            this.Closing -= Window_Closing;

            //Закрытие окна
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
