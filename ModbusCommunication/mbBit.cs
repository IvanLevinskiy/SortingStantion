using System;
using System.ComponentModel;

namespace Communication
{
    /// <summary>
    /// Класс, реализующий модель 
    /// бита в слове
    /// </summary>
    public class mbBit : BitBase
    {
      
        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="bit"></param>
        public mbBit(int bit)
        {
            this.Bit = (byte)bit;
        }

        /// <summary>
        /// Запись значения
        /// </summary>
        /// <param name="value"></param>
        public override bool Write(bool value)
        {
            //Получение нового статуса
            var oldvalue = Convert.ToUInt16(Register.Status);

            //Привидение к типу simatic
            var newvalue = Register.ToSimaticType(oldvalue);

            //Алгоритм установки бита
            if (value == true)
            {
                ushort mask = (ushort)((ushort)(1 << Bit));
                newvalue |=   (ushort)mask;
            }

            //Алгоритм сброса бита
            if (value == false)
            {
                ushort mask =(ushort)( ~(ushort)(1 << Bit));
                newvalue &=  (ushort)mask;
            }

            //Преобразование к типу Simatic
            //с обратным порядком байт
            newvalue = Register.ToSimaticType(newvalue);
            return Register.Write(newvalue);
        }
    }
}
