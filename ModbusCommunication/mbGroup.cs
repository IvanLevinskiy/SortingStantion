using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;

namespace Communication
{
    public class mbGroup
    {
        /// <summary>
        /// Уникальный идентификатор
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
        /// Символьное имя
        /// </summary>
        public string Name
        {
            get
            {
                return $"ГРУППА РЕГИСТРОВ {name}";
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        string name;

        /// <summary>
        /// Свойство для MVVM, указывающее
        /// на то, развернут ли узел
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        bool _isExpanded = false;

        /// <summary>
        /// Указатель на родительское 
        /// устройство
        /// </summary>
        public mbDevice Device;

        /// <summary>
        /// Коллекция регистров
        /// </summary>
        public ObservableCollection<mbRegisterBase> Registers
        {
            get
            {
                return registers;
            }
            set
            {
                registers = value;
            }
        }
        ObservableCollection<mbRegisterBase> registers = new ObservableCollection<mbRegisterBase>();


        /// <summary>
        /// Коллекция активных аварий
        /// </summary>
        public ObservableCollection<mbDiscreteAlarm> ActiveAlarms
        {
            get
            {
                return active_alarms;
            }
            set
            {
                active_alarms = value;
            }
        }
        ObservableCollection<mbDiscreteAlarm> active_alarms = new ObservableCollection<mbDiscreteAlarm>();

        /// <summary>
        /// Метод для добавления новой аварии
        /// </summary>
        /// <param name="alarm"></param>
        public void AddActiveAlarm(mbDiscreteAlarm alarm)
        {
            Action action = () =>
            {
                ActiveAlarms.Insert(0, alarm);
            };
            Device.Server.UI_Ddispather.Invoke(action);
        }

        /// <summary>
        /// Метод для удаления аварии
        /// </summary>
        /// <param name="alarm"></param>
        public void RemoveActiveAlarm(mbDiscreteAlarm alarm)
        {
            Action action = () =>
            {
                ActiveAlarms.Remove(alarm);
            };
            Device.Server.UI_Ddispather.Invoke(action);
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id"></param>
        public mbGroup(string id)
        {
            this.ID = id;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="xmlNode"></param>
        public mbGroup(XmlNode xmlNode)
        {

            //получаем IP
            this.ID = Utilites.GetAttributString(xmlNode, "ID");

            //Проходим по вложенным узлам
            foreach (XmlNode registerNode in xmlNode.ChildNodes)
            {
                //Если массив массив
                if (registerNode.Name.ToUpper() == "ARRAY")
                {
                    CreateArray(registerNode);
                    continue;
                }

                //Если регистр
                if (registerNode.Name.ToUpper() == "REGISTER")
                {
                    this.AddRegisrt(CreateRegister(registerNode));
                    continue;
                }

                //Если аварийный регистр
                if (registerNode.Name.ToUpper() == "ALARMS")
                {
                    this.AddRegisrt(new mbAlarmRegister(registerNode));
                    continue;
                }

                //Если регистр сообщения
                if (registerNode.Name.ToUpper() == "WARNINGS")
                {
                    var alarm = new mbAlarmRegister(registerNode);
                    this.AddRegisrt(alarm);
                    continue;
                }

                //Если строка
                if (registerNode.Name.ToUpper() == "STRING")
                {
                    this.AddRegisrt(new mbString(registerNode));
                    continue;
                }
            }
        }

        /// <summary>
        /// Метод для добавления регистра к устройству
        /// </summary>
        /// <param name="registr"></param>
        public void AddRegisrt(mbRegisterBase registr)
        {
            registr.Group = this;
            Registers.Add(registr);
            //QuantityRegistrs++;
        }

        /// <summary>
        /// Метод для создания массива регистров
        /// </summary>
        /// <param name="xmlNode"></param>
        void CreateArray(XmlNode xmlNode)
        {
            //Получение типа
            var type = Utilites.GetAttributString(xmlNode, "Type").ToUpper();

            //Получение адреса первого регистра
            var startaddress = Utilites.GetAttributInt(xmlNode, "Address");

            //Получение длины массива
            var lenght = Utilites.GetAttributInt(xmlNode, "Lenght");

            //Если регистры типа Real
            if (type.ToUpper() == "REAL")
            {
                int lastindex = startaddress + lenght;

                bool flag = true;

                //Наполнение коллекции регистрами
                for (int i = startaddress; i < startaddress + (lenght * 2); i ++)
                {
                    if (flag == true)
                    {
                        var register = CreateRegister(type, i);
                        register.ReadRequest = false;
                        this.AddRegisrt(register);
                        flag = false;
                        continue;
                    }
                    flag = true;
                }
                var regcount = Registers.Count;
                return;
            }


            //Наполнение коллекции регистрами
            for (int i = startaddress; i < startaddress + lenght; i++)
            {
                var register = CreateRegister(type, i);
                this.AddRegisrt(register);

                //Если длина регистра 4 байта, добавляем 1 к счетчику
            }
        }

        /// <summary>
        /// Метод для создания регистра
        /// </summary>
        /// <param name="type"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        mbRegisterBase CreateRegister(string type, int address)
        {
            switch (type)
            {
                case "SHORT":   return new mbSingleRegister(address.ToString(), Types.Short); break;
                case "USHORT":  return new mbSingleRegister(address.ToString(), Types.Ushort); break;
                case "REAL":    return new mbRealRegister(address.ToString()); break;
                case "FLOAT":   return new mbRealRegister(address.ToString()); break;
                case "ARRAY":   return new mbRealRegister(address.ToString()); break;
            }

            return new mbEmptyRegister(address);
        }

        /// <summary>
        /// Метод для создания регистра
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        mbRegisterBase CreateRegister(XmlNode xmlNode)
        {
            

            //Получаем тип регистра
            var type = Utilites.GetAttributString(xmlNode, "Type").ToUpper();
            var address = Utilites.GetAttributString(xmlNode, "Address");
            var format = Utilites.GetAttributString(xmlNode, "Format");
            var factor = Utilites.GetAttributString(xmlNode, "Factor");
            var hvalue = Utilites.GetAttributString(xmlNode, "HighValue");
            var lvalue = Utilites.GetAttributString(xmlNode, "LowValue");

            //Создаем регистр
            mbRegisterBase register = null;

            //Управляем моделью создания регистра
            switch (type)
            {
                case "SHORT":   register = new mbSingleRegister(address, Types.Short); break;
                case "USHORT":  register = new mbSingleRegister(address, Types.Ushort); break;
                case "REAL":    register = new mbRealRegister(address); break;
                case "FLOAT":   register = new mbRealRegister(address); break;
                case "ARRAY":   register = new mbRealRegister(address); break;
                case "UINT":    register = new mbDoubleRegister(address); break;
                default:        register = new mbEmptyRegister(address); break;
            }

            //Если формат задан, передаем его в регистр
            if (string.IsNullOrEmpty(format) == false)
            {
                register.Format = format;
            }

            //Если коэффициен задан, передаем его в регистр
            if (string.IsNullOrEmpty(factor) == false)
            {
                register.Factor = Convert.ToDouble(factor.Replace(".", ","));
            }

            if (string.IsNullOrEmpty(hvalue) == false)
            {
                register.HightValue = Convert.ToDouble(hvalue.Replace(".", ","));
            }

            if (string.IsNullOrEmpty(lvalue) == false)
            {
                register.LowValue = Convert.ToDouble(lvalue.Replace(".", ","));
            }

            //возвращаем регистр
            return register;
        }

        /// <summary>
        /// Метод для получения списка регистров
        /// </summary>
        /// <returns></returns>
        public List<mbRegisterBase> GetRegistersCollection()
        {
            List<mbRegisterBase> registers = new List<mbRegisterBase>();

            foreach (var register in Registers)
            {
                registers.Add(register);
            }

            return registers;
        }

        #region РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }

        }
        #endregion
    }
}
