using S7Communication;
using System;

namespace SortingStantion.TechnologicalObjects
{

    public class BoxEngine
    {
        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticServer server
        {
            get
            {
                return DataBridge.server;
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
        /// Сигналь от сканера GOODREAD
        /// </summary>
        S7BOOL GOODREAD;

        /// <summary>
        /// Сигналь от сканера NOREAD
        /// </summary>
        S7BOOL NOREAD;

        /// <summary>
        /// Тэг GTIN
        /// </summary>
        S7_STRING GTIN;

        /// <summary>
        /// Тэг ID
        /// </summary>
        S7_STRING BARCODE;



        /// <summary>
        /// Конструктор класса
        /// </summary>
        public BoxEngine()
        {
            //Инициализация сигналов от сканера
            GOODREAD = (S7BOOL)device.GetTagByAddress("DB1.DBX414.0");
            NOREAD   = (S7BOOL)device.GetTagByAddress("DB1.DBX414.1");

            GTIN = (S7_STRING)device.GetTagByAddress("DB1.DBD418-STR14");
            BARCODE = (S7_STRING)device.GetTagByAddress("DB1.DBD434-STR40");

            //Подписываемся на событие по изминению
            //тэга GOODREAD и NOREAD
            GOODREAD.ChangeValue += BARCODESCANER_CHANGEVALUE;
            NOREAD.ChangeValue += BARCODESCANER_CHANGEVALUE;
        }

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
            var task_gtin = "gtin";

            var scaner_barcode = BARCODE.StatusText;
            var task_barcode = "barcode";

            //Если GTIN из сканера и задачи
            //не совпадают - сетим ошибку
            if (scaner_gtin != task_gtin)
            {
                Action action = () =>
                {
                    SortingStantion.frame_gtin_fault.frame_gtin_fault fups = new SortingStantion.frame_gtin_fault.frame_gtin_fault(scaner_gtin, scaner_barcode);
                    fups.Owner = DataBridge.MainScreen;
                    fups.ShowDialog();
                };
                //DataBridge.UIDispatcher.Invoke(action);

                DataBridge.AlarmsEngine.al_1.MessageAction = action;



                DataBridge.AlarmsEngine.al_1.Write(true);
            }    

            //Стираем GOODREAD и NOREAD
            GOODREAD.Write(false);
            NOREAD.Write(false);
        }
    }
}
