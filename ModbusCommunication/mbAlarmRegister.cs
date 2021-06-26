using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;

namespace Communication
{
    public class mbAlarmRegister : mbRegisterBase
    {
        /// <summary>
        /// Коллекция битов
        /// </summary>
        public ObservableCollection<mbDiscreteAlarm> AlarmBits
        {
            get;
            set;
        }

        public mbAlarmRegister()
        { 
        
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbAlarmRegister(XmlNode xmlNode)
        {
            this.ReadRequest = true;

            //Инициализация свойств класса
            Lenght = 2;

            //Инициализация коллекции битов
            AlarmBits = new ObservableCollection<mbDiscreteAlarm>();

            //Получаем свойства
            this.Address = Utilites.GetAttributInt(xmlNode, "Address");
            this.Description = Utilites.GetAttributString(xmlNode, "Description");


            //Процедура для наполнения 
            //коллекции битов
            for (int i = 0; i < 16; i++)
            {
                mbDiscreteAlarm alarm = null;

                //Получение бита аварии
                if (xmlNode.ChildNodes.Count > i)
                {
                    //Получение узла
                    XmlNode node = xmlNode.ChildNodes[i];

                    if (node.Name.ToUpper() == "ALARM")
                    {
                        //Создаем аварию и добавляем её в список
                        alarm = new mbDiscreteAlarm(node);
                        alarm.Register = this;
                        AlarmBits.Add(alarm);

                        continue;
                    }

                    if (node.Name.ToUpper() == "WARNING")
                    {
                        //Создаем аварию и добавляем её в список
                        alarm = new mbDiscreteAlarm(node);
                        alarm.MessageType = MessageType.Message;
                        alarm.Register = this;
                        AlarmBits.Add(alarm);

                        continue;
                    }
                }

                //Создаем аварию и добавляем её в список
                alarm = new mbDiscreteAlarm(i);
                alarm.Register = this;
                AlarmBits.Add(alarm);
            }
        }

        /// <summary>
        /// Метод приводящий значение к строковому виду
        /// </summary>
        public override void UpdatedValue(object value)
        {
            ushort locstatus = Convert.ToUInt16(value);
            StatusText = locstatus.ToString();

            RefrashBits();
        }

        public override void BuildStatus(List<byte> bytes)
        {
            //Если данных меньше двух
            //выходим из функции
            if (bytes.Count < 2)
            {
                return;
            }

            //Построение статуса из байтов
            Status = (UInt16)(bytes[0] << 8) | (UInt16)(bytes[1] << 0);

            //Удаление использованных
            //байтов из коллекции
            bytes.RemoveAt(0);
            bytes.RemoveAt(0);
        }

        /// <summary>
        /// Метод для обновления состояния
        /// битов в регистре
        /// </summary>
        void RefrashBits()
        {
            for (int i = 0; i < 16; i++)
            {
                AlarmBits[i].State = GetBitState(AlarmBits[i].Bit);
            }
        }

        /// <summary>
        /// Метод для получения состояния бита
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        bool GetBitState(byte bit)
        {
            UInt16 word = Convert.ToUInt16(Status);
            word = ToSimaticType(word);
            UInt16 mask = (UInt16)(1 << bit);
            UInt16 temp = (UInt16)(word & mask);
            bool bitstate = temp > 0;
            return bitstate;
        }


        /// <summary>
        /// Метод, возвращающий безнаковое значение
        /// регистра
        /// </summary>
        /// <returns></returns>
        public UInt16 ToUnsigned()
        {
            return Convert.ToUInt16(Status);
        }

        /// <summary>
        /// Метод, возвращающий знаковое значение
        /// регистра
        /// </summary>
        /// <returns></returns>
        public Int16 ToSigned()
        {
            return Convert.ToInt16(Status);
        }
    }
}
