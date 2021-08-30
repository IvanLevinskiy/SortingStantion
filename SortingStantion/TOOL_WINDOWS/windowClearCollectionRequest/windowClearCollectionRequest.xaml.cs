using S7Communication;
using System;
using System.Windows;

namespace SortingStantion.TOOL_WINDOWS.windowClearCollectionRequest
{
    /// <summary>
    /// Логика взаимодействия для windowClearCollectionRequest.xaml
    /// </summary>
    public partial class windowClearCollectionRequest : Window
    {
        /// <summary>
        /// События, вызываемые по клику кнопки
        /// Продолжить
        /// </summary>
        public Action BtnContinueClickEvent;

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticClient server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Количество отсканированных, но не выпущенных
        /// продуктов
        /// </summary>
        S7_DWord ProductCollectionLenght;

        /// <summary>
        /// Тэг для очистки очереди в ПЛК
        /// </summary>
        S7_Boolean CLEAR_ITEMS_COLLECTION_CMD;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public windowClearCollectionRequest(Action btnContinueClickAction)
        {
            //Инициализация UI
            InitializeComponent();

            //Передача события по клику кнопки
            BtnContinueClickEvent = btnContinueClickAction;

            //Инициализация тэга - количество невыпущенных продуктов
            ProductCollectionLenght = (S7_DWord)device.GetTagByAddress("DB5.DBD0-DWORD");

            //Тэг для очистки коллекции изделий
            CLEAR_ITEMS_COLLECTION_CMD = (S7_Boolean)device.GetTagByAddress("DB1.DBX98.2");

            //Если количество невыпущенных продуктов
            //меньше нуля - высываем событие, которое вызывается при
            //нажатии кнопки "ПРОДОЛЖИТЬ"
            UInt32 productquantity = 0;
            var resultconvertion = UInt32.TryParse(ProductCollectionLenght.Status.ToString(), out productquantity);
            if (productquantity == 0)
            {
                BtnContinueClickEvent?.Invoke();
                BtnContinueClickEvent = null;
                this.Close();
                return;
            }

            //Если Невыпущеных продуктов
            //больше 0, вызываем диалог
            this.ShowDialog();
        }

        /// <summary>
        /// Событие, вызваемое при клике по клавише
        /// "ОТМЕНА"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            BtnContinueClickEvent = null;
            this.Close();
        }

        /// <summary>
        /// Событие, вызываемое при нажатии на
        /// клавишу "ПРОДОЛЖИТЬ"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnContinueClick(object sender, RoutedEventArgs e)
        {
            CLEAR_ITEMS_COLLECTION_CMD.Write(true);
            BtnContinueClickEvent?.Invoke();
            BtnContinueClickEvent = null;
            this.Close();
        }
    }
}
