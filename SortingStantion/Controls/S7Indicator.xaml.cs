using S7Communication;
using System;
using System.Windows.Controls;

namespace SortingStantion.Controls
{
    /// <summary>
    /// Логика взаимодействия для S7Indicator.xaml
    /// </summary>
    public partial class S7Indicator : UserControl
    {
        #region SIMATIC СУЩНОСТИ 

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticServer server
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

        #endregion

        /// <summary>
        /// Подпись на изменение тэга
        /// </summary>
        public string S7Operand
        {
            set
            { 
                var tag = (S7_Boolean)device.GetTagByAddress(value);
                tag.ChangeValue += Tag_ChangeValue;
            }
        }

        /// <summary>
        /// Иконка при true
        /// </summary>
        public object IconTrue
        {
            get;
            set;
        }

        /// <summary>
        /// Иконка при false
        /// </summary>
        public object IconFalse
        {
            get;
            set;
        }

        /// <summary>
        /// Метод, вызываемый при изминении тэга
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Tag_ChangeValue(object arg1, object arg2)
        {
            bool value = (bool)arg2;

            Action action = () =>
            {
                //Если тэг изменил состояние на TRUE
                if (value == true)
                {
                    content.Content = IconTrue;
                    return;
                }

                //Если тэг изменил состояние на FALSE
                if (value == false)
                {
                    content.Content = IconFalse;
                    return;
                }
            };

            //Вызов метода в потоке UI
            DataBridge.MainScreen.Dispatcher.Invoke(action);
        }

        /// <summary>
        /// Конструктор класса ИНДИКАТОР
        /// </summary>
        public S7Indicator()
        {
            //Инициализация UI
            InitializeComponent();

            //Метод, вызываемый при загрузке 
            //элемента управлекния
            this.Loaded += (s, e) =>
            {
                content.Content = IconFalse;
            };
            
        }
    }
}
