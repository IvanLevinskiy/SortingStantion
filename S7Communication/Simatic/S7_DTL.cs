using System;

namespace S7Communication
{
    public class S7DTL : simaticTagBase
    {
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7DTL(string name, string address, SimaticGroup simaticGroup)
        {
            this.Name = name;
            this.Address = address;

            this.group = simaticGroup;

            //Количество байт, занимаемых в DB
            Lenght = 12;
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void ObjectToString()
        {
            DateTime dt = (DateTime)Status;
            StatusText = dt.GetDateTimeFormats()[33];
        }

        public override void BuildStatus(byte[] bytes, int offset)
        {
            var bts = GetBytes(bytes, StartByteAddress - offset);

            var year = bts[1] | (bts[0] << 8);
            var month = bts[2];
            var day = bts[3];
            var hour = bts[5];
            var minutes = bts[6];
            var sec = bts[7];

            try
            {
                Status = new DateTime(year, month, day, hour, minutes, sec);
            }
            catch
            {

            }
            
        }
    }
}
