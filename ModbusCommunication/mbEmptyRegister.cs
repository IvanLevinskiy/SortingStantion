using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communication
{
    /// <summary>
    /// Класс, реализующий пустой
    /// регистр для чтения 32 значных
    /// значений
    /// </summary>
    public class mbEmptyRegister : mbRegisterBase
    {
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbEmptyRegister(int address)
        {
            //Инициализация свойств класса
            Address = address;
            Lenght = 0;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="address"></param>
        public mbEmptyRegister(string address)
        {
            //Инициализация свойств класса
            Address = Convert.ToInt16(address);
            Lenght = 0;
        }

        /// <summary>
        /// Метод для получения статуса
        /// </summary>
        /// <param name="bytes"></param>
        public override void BuildStatus(List<byte> bytes)
        {
            //В методе ничего не делаем
            //поскольку это вспомогательный
            //регистр, предназначенный для
            //формирования правильного запроса
        }


    }
}
