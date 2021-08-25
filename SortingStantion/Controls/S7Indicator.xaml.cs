using S7Communication;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace SortingStantion.Controls
{
    /// <summary>
    /// Логика взаимодействия для S7Indicator.xaml
    /// </summary>
    public partial class S7Indicator : UserControl, INotifyPropertyChanged
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

        S7_Boolean tag;

        /// <summary>
        /// Подпись на изменение тэга
        /// </summary>
        public string S7Operand
        {
            set
            { 
                tag = (S7_Boolean)device.GetTagByAddress(value);
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
        /// Содержимое главного экрана
        /// </summary>
        public object xContent
        {
            set
            {
                contentPr.Content = value;
            }
        }
        object _content;

        /// <summary>
        /// Метод, вызываемый при изминении тэга
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Tag_ChangeValue(object arg1, object arg2)
        {
            //Если значение не является bool
            if (arg1 is bool == false)
            {
                return;
            }

            //Приведение типов
            bool oldvalue = (bool)arg1;
            bool newvalue = (bool)arg2;

            Action action = () =>
            {
                //Если тэг изменил состояние на TRUE
                if (newvalue == true)
                {
                    //Костыль
                    tag.Status = true;

                    xContent = IconTrue;

                    return;
                }

                ////Если тэг изменил состояние на FALSE
                if (newvalue == false)
                {
                    //Костыль
                    tag.Status = false;

                    xContent = IconFalse;
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

            DataContext = this;
        }


        #region РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
