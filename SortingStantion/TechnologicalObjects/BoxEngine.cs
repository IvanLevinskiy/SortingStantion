using S7Communication;
using System;

namespace SortingStantion.TechnologicalObjects
{
    /// <summary>
    /// Объяект, осуществляющий работу с коробами, из учет
    /// сравнение для отбраковки
    /// </summary>
    public class BoxEngine
    {
        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticServer server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        public SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Указатель на группу, где хранятся все тэгиК
        /// </summary>
        public SimaticGroup group
        {
            get
            {
                return device.Groups[0];
            }
        }

        /// <summary>
        /// Сигнал от сканера GOODREAD
        /// </summary>
        S7BOOL GOODREAD;

        /// <summary>
        /// Сигналь от сканера NOREAD
        /// </summary>
        S7BOOL NOREAD;

        /// <summary>
        /// Сигнал для перемещения данных
        /// просканированного изделия в коллекцию
        /// </summary>
        S7BOOL TRANSFER_CMD;

        /// <summary>
        /// Тэг GTIN
        /// </summary>
        S7_STRING GTIN;

        /// <summary>
        /// Тэг ID
        /// </summary>
        S7_STRING SERIALNUMBER;


        /// <summary>
        /// Тэг GTIN из задания
        /// </summary>
        S7_STRING GTIN_TASK;


        /// <summary>
        /// Конструктор класса
        /// </summary>
        public BoxEngine()
        {
            //Инициализация сигналов от сканера
            GOODREAD = (S7BOOL)device.GetTagByAddress("DB1.DBX414.0");
            NOREAD   = (S7BOOL)device.GetTagByAddress("DB1.DBX414.1");
            TRANSFER_CMD = (S7BOOL)device.GetTagByAddress("DB1.DBX414.2");

            GTIN = (S7_STRING)device.GetTagByAddress("DB1.DBD416-STR14");
            SERIALNUMBER = (S7_STRING)device.GetTagByAddress("DB1.DBD432-STR6");
            GTIN_TASK = (S7_STRING)device.GetTagByAddress("DB1.DBD226-STR40");

            //Подписываемся на событие по изминению
            //тэга GOODREAD и NOREAD
            GOODREAD.ChangeValue += BARCODESCANER_CHANGEVALUE;
            NOREAD.ChangeValue += BARCODESCANER_CHANGEVALUE;
        }

        /// <summary>
        /// Событие, вызываемое при изменении статуса GOODREAD или NOREAD
        /// </summary>
        /// <param name="svalue"></param>
        private void BARCODESCANER_CHANGEVALUE(object svalue)
        {
            bool? value = (bool?)svalue;

            //Если новое значение не true - выходим
            if (value != true)
            {
                return;
            }

            //Проверяем совпадение GTIN
            var scaner_gtin = GTIN.StatusText;
            var task_gtin = GTIN_TASK.StatusText;

            var scaner_barcode = SERIALNUMBER.StatusText;
            var task_barcode = "barcode";

            //Если GTIN из сканера и задачи
            //не совпадают - сетим ошибку
            if (scaner_gtin != task_gtin)
            {
                DataBridge.AlarmsEngine.al_1.Write(true);
            }
            else
            {
                //Добавляем просканированное изделие
                //в коллекцию изделий результата
                DataBridge.Report.AddBox(scaner_barcode);

                //Взвод флага для перемещения изделия
                //в колекцию коробов между сканером и отбраковщиком
                TRANSFER_CMD.Write(true);
            }

            //Стираем GOODREAD и NOREAD
            GOODREAD.Write(false);
            NOREAD.Write(false);
        }
    }
}
