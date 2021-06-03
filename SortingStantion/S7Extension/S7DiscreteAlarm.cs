using S7Communication;
using SortingStantion.Controls;
using System;

namespace SortingStantion.S7Extension
{
    /// <summary>
    /// Класс, описывающий дисткетное сообщение
    /// </summary>
    public class S7DiscreteAlarm : S7BOOL
    {
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// Метод, вызываемый при возникновлении аварии
        /// </summary>
        public Action MessageAction;

        /// <summary>
        /// Элемент сообщения об ошибке
        /// </summary>
        MSG msg;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="simaticGroup"></param>

        public S7DiscreteAlarm(string message, string address, SimaticGroup simaticGroup) : base(message, address, simaticGroup)
        {
            this.Message = message;
            this.ChangeValue += S7DiscreteAlarm_ChangeValue;
        }

        /// <summary>
        /// Метод, вызываемый при изминении
        /// значения базового тэга
        /// </summary>
        /// <param name="value"></param>
        private void S7DiscreteAlarm_ChangeValue(object value)
        {
            Action action = () =>
            {
                //Если новое значение не bool - выходим
                if (value is bool == false)
                {
                    return;
                }

                //Если новое значение true, добавляем сообщение
                if ((bool)value == true)
                {
                    msg = new MSG(Message, MSGTYPE.ERROR);
                    DataBridge.MSGBOX.Add(msg);

                    MessageAction?.Invoke();

                    return;
                }

                //Если новое значение false, удаляем сообщение
                if ((bool)value == false)
                {
                    DataBridge.MSGBOX.Remove(msg);
                    return;
                }
            };

            DataBridge.UIDispatcher.BeginInvoke(action);

            
        }
    }
}
