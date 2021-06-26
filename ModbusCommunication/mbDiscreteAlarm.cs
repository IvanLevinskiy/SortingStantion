using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace Communication
{
    public class mbDiscreteAlarm : BitBase
    {
        /// <summary>
        /// Время возникновления аварии
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                return dateTime;
            }
            set
            {
                dateTime = value;
                Date = dateTime.ToString("dd.MM.yyyy");
                Time = dateTime.ToString("HH:mm:ss");
                OnPropertyChanged("DateTime");
            }
        }
        DateTime dateTime;

        /// <summary>
        /// Дата возникновления аварии
        /// </summary>
        public string Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
            }
        }
        string date = "10.10.2020";

        /// <summary>
        /// Время возникновления аварии
        /// </summary>
        public string Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
            }
        }
        string time = "10:10:10:10";

        /// <summary>
        /// Идентификатор аварии
        /// </summary>
        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                OnPropertyChanged("ID");
            }
        }
        string id;

        /// <summary>
        /// Тип (категория) сообщения
        /// </summary>
        public MessageType MessageType
        {
            get
            {
                return messageType;
            }
            set
            {
                messageType = value;
                OnPropertyChanged("MessageType");
            }
        }
        MessageType messageType = Communication.MessageType.Alarm;

        /// <summary>
        /// Текстовое сообщение
        /// </summary>
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                OnPropertyChanged("Message");
            }
        }
        string message = "Резерв";
        
        /// <summary>
        /// Имя пользователя, 
        /// при котором возникла авария
        /// </summary>
        public string UserName
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
                OnPropertyChanged("Message");
            }
        }
        string username = "Оператор";

        /// <summary>
        /// Свойство для VM
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        bool _isSelected = false;



        /// <summary>
        /// Конструктор класса
        /// </summary>
        public mbDiscreteAlarm()
        {

        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="bit"></param>
        public mbDiscreteAlarm(int bit)
        {
            this.Bit = (byte)bit;
        }

        /// <summary>
        /// Конструктор класса - дискретная авария
        /// </summary>
        /// <param name="xmlNode"></param>
        public mbDiscreteAlarm(XmlNode xmlNode)
        {
            this.Bit = (byte)Utilites.GetAttributInt(xmlNode, "Bit");
            this.Message = Utilites.GetAttributString(xmlNode, "Message");
            this.ID =       Utilites.GetAttributString(xmlNode, "ID");
        }

        /// <summary>
        /// Метод, вызываемый при изменении данных
        /// </summary>
        /// <param name="value"></param>
        public override void RefreshValue(bool? value)
        {
            //Получение активных аварий
            var activealarms = this.Register.Group.Device.ActiveAlarms;

            //Получение потока UI
            //var main_dispather = this.Register.Group.Device.Server.main_dispather;

            //Если авария возникла, добавляем её в список
            //текущих аварий
            if (value == true)
            {

                 this.DateTime = DateTime.Now;
                 this.Register.Group.Device.AddActiveAlarm(Clone());

                 //Если тип сообщения - авария
                 if (MessageType == MessageType.Alarm)
                 {
                      this.Register.Group.AddActiveAlarm(Clone());
                 }
                    
                return;
            }

            //Если авария пропала, удаляем её из списка
            //текущих аварий
            if (value == false)
            {
                //Ищем аварию по ID, которую следует удалить
                var alarm = findAlarmByID(this, this.Register.Group.Device.ActiveAlarms);

                //Удаляем аварию из коллекции активных аварий устройства
                this.Register.Group.Device.ActiveAlarms.Remove(alarm);

                //Ищем аварию по ID, которую следует удалить
                alarm = findAlarmByID(this, this.Register.Group.ActiveAlarms);

                //Удаляем аварию из коллекции активных аварий группы регистров
                this.Register.Group.RemoveActiveAlarm(alarm);
            }

        }

        /// <summary>
        /// Поиск аварии из коллекции
        /// по ID
        /// </summary>
        /// <returns></returns>
        public mbDiscreteAlarm findAlarmByID(mbDiscreteAlarm ialarm, ObservableCollection<mbDiscreteAlarm> alarmscollections)
        {
            foreach (var alarm in alarmscollections)
            {
                if (alarm.ID == ialarm.ID)
                {
                    return alarm;
                }
            }

            return null;
        }

        /// <summary>
        /// Метод для клонирования экземпляра аварии
        /// </summary>
        /// <returns></returns>
        public mbDiscreteAlarm Clone()
        {
            var clone = new mbDiscreteAlarm();
            clone.ID = this.ID;
            clone.Message = this.Message;
            clone.MessageType = this.MessageType;
            clone.DateTime = this.DateTime;
            clone.Date = this.Date;
            clone.Time = this.Time;
            clone.UserName = this.UserName;

            return clone;
        }

    }
}
