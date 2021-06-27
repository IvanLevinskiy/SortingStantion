using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using System.Windows.Media;

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
        /// Результат сканирования
        /// </summary>
        S7_CHARS_ARRAY SCAN_DATA;

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
            GOODREAD = (S7BOOL)device.GetTagByAddress("DB1.DBX414.0");
            NOREAD   = (S7BOOL)device.GetTagByAddress("DB1.DBX414.1");
            TRANSFER_CMD = (S7BOOL)device.GetTagByAddress("DB1.DBX414.2");

            GTIN = (S7_STRING)device.GetTagByAddress("DB1.DBD416-STR14");
            SERIALNUMBER = (S7_STRING)device.GetTagByAddress("DB1.DBD432-STR6");
            GTIN_TASK = (S7_STRING)device.GetTagByAddress("DB1.DBD226-STR40");

            //Данные из сканера
            SCAN_DATA = (S7_CHARS_ARRAY)device.GetTagByAddress("DB9.DBD14-CHARS100");
            //SCAN_DATA.Write("010460123456789521F&8h3W93h(0F");

            //Подписываемся на событие по изминению
            //тэга GOODREAD и NOREAD  и осуществляем вызов
            //метода в потоке UI
            GOODREAD.ChangeValue += (ov, nv) =>
            {
                Action action = () =>
                {
                    BARCODESCANER_CHANGEVALUE(ov, nv);
                };
                DataBridge.MainScreen.Dispatcher.Invoke(action);
            };
            
            //NOREAD.ChangeValue += BARCODESCANER_CHANGEVALUE;
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
            NOREAD.Write(false);

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

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"От автоматического сканера получен посторонний код: {spliter.SourseData}  (код не является СИ)", MessageType.Alarm);

                //Вывод сообщения в зоне информации
                string message = $"Посторонний код (код не является СИ)";
                var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xDB, 0x49, 0x69));
                var msg = new UserMessage(message, brush);
                DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                SortingStantion.TOOL_WINDOWS.windowExtraneousBarcode.windowExtraneousBarcode windowExtraneousBarcode = new SortingStantion.TOOL_WINDOWS.windowExtraneousBarcode.windowExtraneousBarcode();
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

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Посторонний продукт (GTIN не совпадает с заданием)", MessageType.Alarm);

                //Вывод сообщения в зоне информации
                string message = $"Посторонний продукт (GTIN не совпадает с заданием)";
                var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xDB, 0x49, 0x69));
                var msg = new UserMessage(message, brush);
                DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                SortingStantion.TOOL_WINDOWS.windowExtraneousBarcode.windowExtraneousBarcode windowExtraneousBarcode = new SortingStantion.TOOL_WINDOWS.windowExtraneousBarcode.windowExtraneousBarcode();
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
                var msg = new UserMessage(message, MSGTYPE.ERROR);
                DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                //SortingStantion.TOOL_WINDOWS.windowExtraneousBarcode.windowExtraneousBarcode windowExtraneousBarcode = new SortingStantion.TOOL_WINDOWS.windowExtraneousBarcode.windowExtraneousBarcode();
                //windowExtraneousBarcode.ShowDialog();

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

            //Взвод флага для перемещения изделия
            //в колекцию коробов между сканером и отбраковщиком
            TRANSFER_CMD.Write(true);
        }
    }
}
