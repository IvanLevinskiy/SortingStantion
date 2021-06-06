using S7Communication;
using SortingStantion.Controls;
using SortingStantion.S7Extension;
using System;

namespace SortingStantion.Models
{
    public class AlarmsEngine
    {

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticServer server
        {
            get
            {
                return DataBridge.server;
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
        public S7BOOL ResetAlarmsTag;

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
        /// Ошибка 9 - Ошибка отбраковщика (продукт не отбраковался)
        /// </summary>
        public S7DiscreteAlarm al_9;

        /// <summary>
        /// Ошибка 13 - Ошибка ИБП UPS
        /// </summary>
        public S7DiscreteAlarm al_13;

        /// <summary>
        /// Сообщение о том, что соединение с ПЛК потеряно
        /// </summary>
        Controls.UserMessage msgLostConnection;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public AlarmsEngine()
        {
            ResetAlarmsTag = (S7BOOL)device.GetTagByAddress("DB1.DBX132.1");

            //Сбрасываем ошибки
            ResetAlarmsTag.Write(true);

            //Посторонний продукт
            al_1 = new S7DiscreteAlarm("Посторонний продукт (GTIN не совпадает с заданием)", "DB6.DBX12.0", group);
            al_1.MessageAction = () =>
            {
                frame_gtin_fault.frame_gtin_fault fr= new frame_gtin_fault.frame_gtin_fault();
                fr.Owner = DataBridge.MainScreen;
                fr.ShowDialog();
            };



            al_2 = new S7DiscreteAlarm("Посторонний код (код не является СИ)", "DB6.DBX12.1", group);



            al_3 = new S7DiscreteAlarm("Номер продукта числится в браке", "DB6.DBX12.2", group);
            al_4 = new S7DiscreteAlarm("Повтор кода продукта", "DB6.DBX12.3", group);
            al_5 = new S7DiscreteAlarm("Получение кода от сканера при остановке конвейера", "DB6.DBX12.4", group);
            al_6 = new S7DiscreteAlarm("Ошибка отбраковщика (продукт не отбраковался)", "DB6.DBX12.5", group);
            al_7 = new S7DiscreteAlarm("Массовый брак", "DB6.DBX12.6", group);

            //Ошибка питания UPS (ИБП)
            al_13 = new S7DiscreteAlarm("Потеря питания! Комплекс будет отключен", "DB6.DBX13.4", group);
            al_13.MessageAction = () =>
            {
                SortingStantion.frameUSPFault.frameUSPFault fups = new SortingStantion.frameUSPFault.frameUSPFault();
                fups.Owner = DataBridge.MainScreen;
                fups.ShowDialog();
            };


            //Ошибка потреи связи с устройством
            device.LostConnection += () =>
            {
                Action action = () =>
                {
                    msgLostConnection = new Controls.UserMessage($"Потеряно соединение с ПЛК {device.IP}", MSGTYPE.ERROR);
                    DataBridge.MSGBOX.Add(msgLostConnection);
                };
                DataBridge.UIDispatcher.Invoke(action);
                
            };

            //Ошибка потреи связи с устройством
            device.GotConnection += () =>
            {
                Action action = () =>
                {
                    DataBridge.MSGBOX.Remove(msgLostConnection);
                };
                DataBridge.UIDispatcher.Invoke(action);

            };

        }
    }
}
