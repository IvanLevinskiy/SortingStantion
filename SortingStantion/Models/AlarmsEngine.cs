using S7Communication;
using SortingStantion.Controls;
using SortingStantion.S7Extension;
using SortingStantion.TOOL_WINDOWS.windowOverDeffectCounter;
using SortingStantion.TOOL_WINDOWS.windowProductsAreTooCloseToEachOther;
using SortingStantion.TOOL_WINDOWS.windowPusherError;
using System;

namespace SortingStantion.Models
{
    public class AlarmsEngine
    {

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticClient server
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

        /// <summary>
        /// Указатель на группу, где хранятся все тэгиК
        /// </summary>
        SimaticGroup group
        {
            get
            {
                return device.Groups[0];
            }
        }

        /// <summary>
        /// Тэ для сброса ошибок
        /// </summary>
        public S7_Boolean ResetAlarmsTag;

        /// <summary>
        /// Ошибка 1 - Посторонний продукт (GTIN не совпадает с заданием)
        /// </summary>
        public S7DiscreteAlarm al_1;

        /// <summary>
        /// Ошибка 2 - Посторонний код (код не является СИ)
        /// </summary>
        public S7DiscreteAlarm al_2;

        /// <summary>
        /// Ошибка 3 - Номер продукта числится в браке
        /// </summary>
        public S7DiscreteAlarm al_3;

        /// <summary>
        /// Ошибка 4 - Повтор кода продукта
        /// </summary>
        public S7DiscreteAlarm al_4;

        /// <summary>
        /// Ошибка 5 - Получение кода от сканера при остановке конвейера
        /// </summary>
        public S7DiscreteAlarm al_5;

        /// <summary>
        /// Ошибка 6 - Ошибка отбраковщика (продукт не отбраковался)
        /// </summary>
        public S7DiscreteAlarm al_6;

        /// <summary>
        /// Ошибка 7 - Массовый брак
        /// </summary>
        public S7DiscreteAlarm al_7;

        /// <summary>
        /// Ошибка 8 - Ошибка отбраковщика
        /// </summary>
        public S7DiscreteAlarm al_8;

        /// <summary>
        /// Ошибка 9 - Продукт слишком близко друг другу
        /// </summary>
        public S7DiscreteAlarm al_9;

        /// <summary>
        /// Ошибка 13 - Ошибка ИБП UPS
        /// </summary>
        public S7DiscreteAlarm al_13;

        /// <summary>
        /// Количесво изделий,
        /// отбракованых вручную
        /// </summary>
        public S7_DWord QUANTITY_PRODUCTS_MANUAL_REJECTED;

        /// <summary>
        /// Счетчик брака
        /// </summary>
        public S7_DWord DEFFECT_PRODUCTS_COUNTER;


        /// <summary>
        /// Сообщение о том, что соединение с ПЛК потеряно
        /// </summary>
        Controls.UserMessage msgLostConnection;

        /// <summary>
        /// Окно ошибки "МАССОВЫЙ БРАК"
        /// </summary>
        windowOverDeffectCounter windowOverDeffectCounter;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public AlarmsEngine()
        {
            //Инициализация тэга по которому осуществляется сброс
            //всех сообщений
            ResetAlarmsTag = (S7_Boolean)device.GetTagByAddress("DB1.DBX132.1");

            //Тэг, хранящий количество изделий, отбраковыных вручную
            QUANTITY_PRODUCTS_MANUAL_REJECTED = (S7_DWord)device.GetTagByAddress("DB1.DBD28-DWORD");

            //Тэг, хранящий счетчик подряд отбракованых продуктов
            DEFFECT_PRODUCTS_COUNTER = (S7_DWord)device.GetTagByAddress("DB1.DBD32-DWORD");

            /*
                Неисправность фотодатчика FS1
            */
            al_1 = new S7DiscreteAlarm("Отсутствует связь с датчиком сканера, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", "DB6.DBX12.0", group);
            al_1.MessageAction = () =>
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Неисправность фотодатчика FS1 (перед сканером)", MessageType.Alarm);
            };


            /*
                Неисправность фотодатчика FS2
            */
            al_2 = new S7DiscreteAlarm("Отсутствует связь с датчиком отбраковщика, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", "DB6.DBX12.1", group);
            al_2.MessageAction = () =>
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Неисправность фотодатчика FS2 (перед отбраковщиком)", MessageType.Alarm);

            };

            /*
                Неисправность фотодатчика FS3
            */
            al_3 = new S7DiscreteAlarm("Отсутствует связь с датчиком контроля отбраковки, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", "DB6.DBX12.2", group);
            al_3.MessageAction = () =>
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Неисправность фотодатчика FS3 (перед отбраковщиком)", MessageType.Alarm);
            };

            /*
                Повтор кода продукта
  w

            /*
                Получение кода от сканера при остановке конвейера
            */
            //al_5 = new S7DiscreteAlarm("Получение кода от сканера при остановке конвейера", "DB6.DBX12.4", group);
            //al_5.MessageAction = () =>
            //{

            //    //Запись сообщения в базу данных
            //    DataBridge.AlarmLogging.AddMessage("Получение кода от сканера при остановке конвейера", MessageType.Alarm);
            //};

            /*
                Ошибка отбраковщика (продукт не отбраковался)
            */
            al_6 = new S7DiscreteAlarm("Ошибка отбраковщика (продукт не отбраковался)", "DB6.DBX12.5", group);
            al_6.MessageAction = () =>
            {
                //Если линия выключена
                if (DataBridge.Conveyor.LineIsRun == false)
                {
                    //al_6.Write(false);
                    //return;
                }

                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Ошибка отбраковщика (продукт не отбраковался)", MessageType.Alarm);

                //Вывод окна ошибки
                windowPusherError windowPusherError = new windowPusherError(al_6);
                windowPusherError.ShowDialog();
            };

            /*
                Массовый брак
            */
            al_7 = new S7DiscreteAlarm("Массовый брак", "DB6.DBX12.6", group);
            al_7.MessageAction = () =>
            {
                //Если линия выключена
                if (DataBridge.Conveyor.LineIsRun == false)
                {
                    //return;
                }

                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Массовый брак", MessageType.Alarm);

                //Вывод окна ошибки
                //if (windowOverDeffectCounter == null)
                //{
                    windowOverDeffectCounter = new windowOverDeffectCounter(DEFFECT_PRODUCTS_COUNTER, al_7);
                    windowOverDeffectCounter.ShowDialog();
                    //windowOverDeffectCounter = null;
                //}
            };

            /*
                Продукт слишком близко к предыдущему, удалите его с конвейера
            */
            al_9 = new S7DiscreteAlarm("Продукт слишком близко к предыдущему, удалите его с конвейера", "DB6.DBX13.0", group);
            al_9.MessageAction = () =>
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Продукт слишком близко к предыдущему", MessageType.Alarm);

                //Сброс ошибки
                al_9.Write(false);

                //Вывод окна ошибки
                windowProductsAreTooCloseToEachOther windowProductsAreTooCloseToEachOther = new windowProductsAreTooCloseToEachOther();
                windowProductsAreTooCloseToEachOther.ShowDialog();
            };


            /*
                Ошибка питания UPS (ИБП)
            */
            al_13 = new S7DiscreteAlarm("Потеря питания! Комплекс будет отключен", "DB6.DBX13.4", group);
            al_13.MessageAction = () =>
            {
                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Ошибка питания ИБП", MessageType.Alarm);

                //Создание окна
                SortingStantion.frameUSPFault.frameUSPFault fups = new SortingStantion.frameUSPFault.frameUSPFault();
                fups.Owner = DataBridge.MainScreen;
                fups.ShowDialog();
            };

            /*
                Ошибка потреи связи с устройством
            */
            device.LostConnection += () =>
            {
                Action action = () =>
                {
                    //Запись сообщения в базу данных
                    DataBridge.AlarmLogging.AddMessage("Потеря связи с ПЛК", MessageType.Alarm);

                    msgLostConnection = new Controls.UserMessage($"Линия остановлена. Обрыв связи с контроллером.", MSGTYPE.ERROR);
                    DataBridge.MSGBOX.Add(msgLostConnection);
                };
                DataBridge.UIDispatcher?.Invoke(action);
            };

            /*
                Ошибка потреи связи с устройством
            */
            device.GotConnection += () =>
            {
                Action action = () =>
                {
                    //Запись сообщения в базу данных
                    DataBridge.AlarmLogging.AddMessage("Восстановление связи с ПЛК", MessageType.Alarm);

                    DataBridge.MSGBOX.Remove(msgLostConnection);
                };
                DataBridge.UIDispatcher?.Invoke(action);

            };

        }
    }
}
