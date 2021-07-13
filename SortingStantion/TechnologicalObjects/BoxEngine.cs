using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using System.Windows.Media;
using SortingStantion.TOOL_WINDOWS.windowGtinFault;
using SortingStantion.TOOL_WINDOWS.windowExtraneousBarcode;
using SortingStantion.TOOL_WINDOWS.windowProductIsDeffect;
using SortingStantion.TOOL_WINDOWS.windowRepeatProduct;

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
        /// Результат сканирования
        /// </summary>
        S7_CHARS_ARRAY SCAN_DATA;

        /// <summary>
        /// Тэг - разрешить повтор кода продукта
        /// </summary>
        S7BOOL REPEAT_ENABLE;

        /// <summary>
        /// Тэг, указывающий на то, что отбраковка продукта
        /// не нужна
        /// </summary>
        S7BOOL IS_GOOD_FLAG;


        /// <summary>
        /// Объект, осуществляющий разбор телеграммы
        /// сканированного штрихкода
        /// </summary>
        DataSpliter spliter = new DataSpliter();

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public BoxEngine()
        {
            //Инициализация сигналов от сканера
            GOODREAD = (S7BOOL)device.GetTagByAddress("DB1.DBX380.0");
            NOREAD   = (S7BOOL)device.GetTagByAddress("DB1.DBX380.1");
            TRANSFER_CMD = (S7BOOL)device.GetTagByAddress("DB1.DBX380.2");
            IS_GOOD_FLAG = (S7BOOL)device.GetTagByAddress("DB1.DBX380.3");
            REPEAT_ENABLE = (S7BOOL)device.GetTagByAddress("DB1.DBX134.0");

            GTIN = (S7_STRING)device.GetTagByAddress("DB1.DBD382-STR14");
            SERIALNUMBER = (S7_STRING)device.GetTagByAddress("DB1.DBD398-STR6");
     
            //Данные из сканера
            SCAN_DATA = (S7_CHARS_ARRAY)device.GetTagByAddress("DB9.DBD14-CHARS100");
            //SCAN_DATA.Write("010460456789012621F&8h3W93h(0F");

            //Подписываемся на событие по изминению
            //тэга GOODREAD и NOREAD  и осуществляем вызов
            //метода в потоке UI
            GOODREAD.ChangeValue += (ov, nv) =>
            {
                //Если новое или старое значение не bool
                var errortr = (ov is bool) == false;
                errortr = errortr || (nv is bool) == false;

                if (errortr == true)
                {
                    return;
                }

                //В случае, если ппроисходит 
                //сброс тэга - код не выполняем
                if ((bool)ov == false && (bool)nv == true )
                {
                    Action action = () =>
                    {
                        BARCODESCANER_CHANGEVALUE(ov, nv);
                    };
                    DataBridge.MainScreen.Dispatcher.Invoke(action);
                }

                
            };

            NOREAD.ChangeValue += (ov, nv) =>
            {
                //Если новое или старое значение не bool
                var errortr = (ov is bool) == false;
                errortr = errortr || (nv is bool) == false;

                if (errortr == true)
                {
                    return;
                }

                //В случае, если ппроисходит 
                //сброс тэга - код не выполняем
                if ((bool)ov == false && (bool)nv == true)
                {
                    
                    //Стирание данных из рузультата сканирования
                    GTIN.Write(string.Empty);
                    SERIALNUMBER.Write(string.Empty);

                    IS_GOOD_FLAG.Write(false);

                    //Взвод флага для перемещения изделия
                    //в колекцию коробов между сканером и отбраковщиком
                    TRANSFER_CMD.Write(true);

                    //Стираем GOODREAD и NOREAD
                    //для того, чтоб процедура отработала один раз
                    NOREAD.Write(false);
                }

                
            };
        }

        /// <summary>
        /// Событие, вызываемое при изменении статуса GOODREAD или NOREAD
        /// </summary>
        /// <param name="svalue"></param>
        private void BARCODESCANER_CHANGEVALUE(object oldvalue, object newvalue)
        {
            bool? value = (bool?)newvalue;

            //Если новое значение не true - выходим
            if (value != true)
            {
                return;
            }

            //Стираем GOODREAD и NOREAD
            //для того, чтоб процедура отработала один раз
            GOODREAD.Write(false);
            //IS_GOOD_FLAG.Write(false);

            /*
                Если линия не в работе (определяется по таймеру остановки в TIA) 
            */
            if ((bool)DataBridge.Conveyor.IsStopFromTimerTag.Status == true)
            {
               
                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"Получен штрихкод при остановленной линии", MessageType.Alarm);

                //Вывод сообщения в зоне информации
                string message = $"Конвейер не запущен, полученный код не будет записан в результат";
                var msg = new UserMessage(message, DataBridge.myRed);
                DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                customMessageBox mb = new customMessageBox("Ошибка", "Подтвердите удаление продукта с конвейера!");
                mb.Owner = DataBridge.MainScreen;
                mb.ShowDialog();


                //Выход из метода
                return;

            }

            //Получение GTIN из задания
            var task_gtin = DataBridge.WorkAssignmentEngine.GTIN;

            //Разбор телеграммы из ПЛК
            var data = SCAN_DATA.StatusText;
            spliter.Split(ref data);

            //Получение GTIN и SerialNumber из разобранного
            //штрихкода, полученного из ПЛК
            var scaner_serialnumber = spliter.SerialNumber;
            var scaner_gtin = spliter.GTIN;

            /*
                Если сигнатура задачи не совпадает
                с той, что указана в задании
            */
            if (spliter.IsValid == false)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"От автоматического сканера получен посторонний код: {spliter.SourseData}  (код не является СИ)", MessageType.Alarm);

                //Вывод сообщения в зоне информации
                string message = $"Посторонний код (код не является СИ)";
                var msg = new UserMessage(message, DataBridge.myRed);
                DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowExtraneousBarcode = new windowExtraneousBarcode(msg);
                windowExtraneousBarcode.ShowDialog();

                //Выход из метода
                return;
            }

            /*
                Если GTIN из сканера и задачи
                не совпадают - формируем ошибку
            */
            if (scaner_gtin != task_gtin)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Посторонний продукт (GTIN не совпадает с заданием)", MessageType.Alarm);

                //Вывод сообщения в зоне информации
                string message = $"Посторонний продукт (GTIN не совпадает с заданием)";
                var msg = new UserMessage(message, DataBridge.myRed);
                DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowExtraneousBarcode = new windowGtinFault(scaner_gtin, scaner_serialnumber, msg);
                windowExtraneousBarcode.ShowDialog();

                //Выход из метода
                return;
            }

            /*
                Если продукт числится в браке
            */
            if (DataBridge.Report.IsDeffect(scaner_serialnumber) == true)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"Номер продукта {scaner_serialnumber} числится в браке", MessageType.Alarm);

                //Вывод сообщения в окно информации
                string message = $"Номер продукта {scaner_serialnumber} числится в браке";
                var msg = new UserMessage(message, DataBridge.myRed);
                DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowProductIsDeffect = new windowProductIsDeffect(scaner_serialnumber, msg);
                windowProductIsDeffect.ShowDialog();

                //Выход из метода
                return;
            }


            /*
                Повтор кода запрещен
            */
            var RepeatEnable = (bool)REPEAT_ENABLE.Status;
            var IsRepeat = DataBridge.Report.IsRepeat(scaner_serialnumber);
            if (RepeatEnable == false && IsRepeat == true)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"Продукт GTIN {scaner_gtin} номер {scaner_serialnumber} считан повторно", MessageType.Alarm);

                //Вывод сообщения в окно информации
                string message = $"Продукт GTIN {scaner_gtin} номер {scaner_serialnumber} считан повторно";
                var msg = new UserMessage(message, DataBridge.myRed);
                DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowRepeatProduct = new windowRepeatProduct(scaner_gtin, scaner_serialnumber, msg);
                windowRepeatProduct.ShowDialog();

                //Выход из метода
                return;
            }


            /*
                Если проверка прошла успешно добавляем 
                продукт в результат
            */

            //Добавляем просканированное изделие
            //в коллекцию изделий результата
            DataBridge.Report.AddBox(scaner_serialnumber);

            //Запись текущих GTIN и SerialNumber в ПЛК
            GTIN.Write(scaner_gtin);
            SERIALNUMBER.Write(scaner_serialnumber);
            IS_GOOD_FLAG.Write(true);

            //Взвод флага для перемещения изделия
            //в колекцию коробов между сканером и отбраковщиком
            TRANSFER_CMD.Write(true);
        }
    }
}
