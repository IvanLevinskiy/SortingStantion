using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SortingStantion.Controls
{
    public class S7_FIELD_IO : TextBox, INotifyPropertyChanged
    {
        /// <summary>
        /// Указатель на оригинальный 
        /// контролл
        /// </summary>
        TextBox originalTextBox;

        /// <summary>
        /// Адрес регистра
        /// </summary>
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;

                //Если сервер не проинициализирован - инициализируем его
                if (DataBridge.S7Server == null)
                {
                    DataBridge.CreateSimaticServer();
                }

                S7TAG = DataBridge.S7Server.Devices[0].GetTagByAddress(address);

                //Если регистр не найден
                if (S7TAG == null)
                {
                    return;
                }

                //Подписка на изменение значение тэга
                S7TAG.ChangeValue += S7TAG_ChangeValue;

                //Подписка на возобновление соединения с ПЛК
                S7TAG.device.GotConnection += Device_GotConnection;

            }
        }


        string address;

        /// <summary>
        /// Текст всплывающей подсказки
        /// для пользователя
        /// </summary>
        public string TooltipUser
        {
            get;
            set;
        }

        /// <summary>
        /// Текст всплывающей подсказки
        /// для администратора
        /// </summary>
        private string TooltipAdmin
        {
            get;
            set;
        }

        /// <summary>
        /// Флаг, разрешающий ведение лога
        /// </summary>
        bool IsLogging;

        /// <summary>
        /// Экземпляр регистра, связанного с полем
        /// </summary>
        public S7Communication.simaticTagBase S7TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Тип переменной
        /// </summary>
        Type Type;

        string text;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7_FIELD_IO()
        {
            //Передача контекста данных
            DataContext = this;

            //Подписка на событие по загрузке контролла
            this.Loaded += MbTextBox_Loaded;
        }

        /// <summary>
        /// Метод для поиска родителя, реализующего
        /// интерфейс IMbScreen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            //Получение оригинального текстбокса
            this.originalTextBox = (TextBox)e.OriginalSource;

            //Подписка на события
            this.LostKeyboardFocus += MbTextBox_LostKeyboardFocus;
            this.KeyDown += MbTextBox_KeyDown;
            this.GotKeyboardFocus += MbTextBox_GotKeyboardFocus;
            this.TextChanged += MbTextBox_TextChanged;
        }


        /// <summary>
        /// Метод, вызываемый при изменении текста
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (S7TAG == null)
            {
                return;
            }

            //Ограничение ввода символов 
            var os = (TextBox)e.OriginalSource;
        }

        /// <summary>
        /// Обработчик события по изминению
        /// состояния тэга
        /// </summary>
        /// <param name="obj"></param>
        private void S7TAG_ChangeValue(object obj)
        {
            SetText(S7TAG.StatusText);
        }


        /// <summary>
        /// При потере фокуса клавиатурой
        /// начинаем обновлять значения в текстоксе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (S7TAG == null)
            {
                return;
            }

            //Получаем экземпляр TextBox
            TextBox originalsourse = (TextBox)e.OriginalSource;

            //Запоинаем текст в контроле
            originalsourse.Text = text;

            //Возвращаем подписку на изменение регистра
            S7TAG.ChangeValue += S7TAG_ChangeValue;
        }

        /// <summary>
        /// Запись в регистр значения по 
        /// нажатию на кнопку Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Получение оригинального источника
            TextBox originalsourse = (TextBox)e.OriginalSource;
            this.originalTextBox = originalsourse;

            //При нажатии на кнопку ввода
            //записываем значение в регистр
            if (e.Key == Key.Enter)
            {
                this.Text = originalsourse.Text;

                //Если регистра нет, пишем по умолчанию ***
                if (S7TAG == null)
                {
                    SetText("***");
                    deFocus();
                    return;
                }

                //Запоинаем текст в контроле
                if ((bool)S7TAG?.Write(this.Text))
                {
                    text = Text;
                    deFocus();
                }

                return;
            }

            //При нажатии на кнопку эскейт
            //возвращаем значение
            if (e.Key == Key.Escape)
            {
                S7TAG_ChangeValue(true);
                //DataBridge.FocusElement.Focus();
            }
        }

        /// <summary>
        /// Метод для перемещения фокуса 
        /// на другой элемент управления
        /// </summary>
        void deFocus()
        {
            //Перевод фокуса на другой элемент
            var parent = (UIElement)this.Parent;
            parent.Focusable = true;
            parent.Focus();
        }


        /// <summary>
        /// Метод для установки текста в контролле
        /// </summary>
        void SetText(string text)
        {
            Action action = () =>
            {
                if (this.originalTextBox == null)
                {
                    this.Text = text;

                }
                else
                {
                    this.Text = text;
                    this.originalTextBox.Text = text;
                }
            };

            this.Dispatcher.Invoke(action);
        }


        /// <summary>
        /// Метод, вызываемый при возобновлении
        /// соединения с ПЛК
        /// </summary>
        private void Device_GotConnection()
        {
            Action action = () =>
            {
                //Преобразование к типу sting
                S7TAG.ObjectToString();

                this.Text = S7TAG.StatusText;
                //this.originalTextBox.Text = S7TAG.StatusText;
            };

            this.Dispatcher.Invoke(action);
        }

        /// <summary>
        /// При получения фокуса ввода 
        /// с клавиатуры отписываемся
        /// от изминения текста по тэгу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbTextBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            //Получаем экземпляр TextBox
            TextBox originalsourse = (TextBox)e.OriginalSource;

            //Запоинаем текст в контроле
            text = originalsourse.Text;

            //Отписываемся от обновления 
            //значения тэга
            if (S7TAG != null)
            {
                S7TAG.ChangeValue -= S7TAG_ChangeValue;
            }

 
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
