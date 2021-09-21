﻿using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using System.Windows.Media;
using SortingStantion.ToolsWindows.windowGtinFault;
using SortingStantion.ToolsWindows.windowExtraneousBarcode;
using SortingStantion.ToolsWindows.windowProductIsDeffect;
using SortingStantion.ToolsWindows.windowRepeatProduct;
using System.Windows.Input;
using SortingStantion.Utilites;

namespace SortingStantion.TechnologicalObjects
{
    /// <summary>
    /// Объяект, осуществляющий работу с продуктами, их учет
    /// сравнение для отбраковки
    /// </summary>
    public class ProductsEngine
    {
        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticClient server
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
        /// Сигнал для перемещения данных
        /// просканированного изделия в коллекцию
        /// </summary>
        S7_Boolean READ_CMD;

        /// <summary>
        /// Тэг для очистки очереди в ПЛК
        /// </summary>
        S7_Boolean CLEAR_ITEMS_COLLECTION_CMD;

        /// <summary>
        /// Результат сканирования
        /// </summary>
        S7_String SCAN_DATA;

        /// <summary>
        /// Тэг - разрешить повтор кода продукта
        /// </summary>
        S7_Boolean REPEAT_ENABLE;

        /// <summary>
        /// Количество отсканированных, но не выпущенных
        /// продуктов
        /// </summary>
        S7_DWord ProductCollectionLenght;

        /// <summary>
        /// Счетчик повтора кодов
        /// </summary>
        S7_DWord RepeatProductCounter;

        /// <summary>
        /// Объект, осуществляющий разбор телеграммы
        /// сканированного штрихкода
        /// </summary>
        DataSpliter spliter = new DataSpliter();

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public ProductsEngine()
        {
            //Инициализация сигналов от сканера
            REPEAT_ENABLE = (S7_Boolean)device.GetTagByAddress("DB1.DBX134.0");

            //Команда для считывания кода сканера
            READ_CMD = (S7_Boolean)device.GetTagByAddress("DB1.DBX378.0");

            //Тэг для очистки коллекции изделий
            CLEAR_ITEMS_COLLECTION_CMD = (S7_Boolean)device.GetTagByAddress("DB1.DBX98.2");

            //Данные из сканера
            SCAN_DATA = (S7_String)device.GetTagByAddress("DB1.DBD506-STR100");

            //Количество отсканированных но не выпущенных объектов
            ProductCollectionLenght = (S7_DWord)device.GetTagByAddress("DB5.DBD0-DWORD");

            //Счетчик повторов
            RepeatProductCounter = (S7_DWord)device.GetTagByAddress("DB1.DBD36-DWORD");

            //Подписываемся на событие по изминению
            //тэга READ_CMD  и осуществляем вызов
            //метода в потоке UI
            READ_CMD.ChangeValue += (ov, nv) =>
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
                    SCAN_DATA.DataUpdated += SCAN_DATA_DataUpdated;
                }
            };

            //При первом скане очищаем коллекцию
            //продуктов в очереди ПЛК
            device.FirstScan += () =>
            {
                //Очистка коллекции продуктов в очереди ПЛК
                CLEAR_ITEMS_COLLECTION_CMD.Write(true);
            };
        }

        /// <summary>
        /// Метод, вызываемый при обновлении массива данных
        /// от сканера
        /// </summary>
        /// <param name="obj"></param>
        private void SCAN_DATA_DataUpdated(object obj)
        {
            //В случае, если ппроисходит 
            //сброс тэга - код не выполняем
            Action action = () =>
            {
                BARCODESCANER_CHANGEVALUE(null, null);
                SCAN_DATA.DataUpdated -= SCAN_DATA_DataUpdated;
            };
            DataBridge.MainScreen.Dispatcher.Invoke(action);
        }

        /// <summary>
        /// Событие, вызываемое при изменении статуса GOODREAD или NOREAD
        /// </summary>
        /// <param name="svalue"></param>
        private void BARCODESCANER_CHANGEVALUE(object oldvalue, object newvalue)
        {
            //Стираем флаг READ_CMD
            //для того, чтоб процедура отработала один раз
            READ_CMD.Write(false);


            //Получение статуса линии
            //с учетом таймера, учитывающего время
            //остановки линии
            var lineistop = false;

            lineistop = (bool)DataBridge.Conveyor.IsStopFromTimerTag.Value;

            /*
                Если линия не в работе (определяется по таймеру остановки в TIA) 
            */
            //if (lineistop == true)
            //{
               
            //    //Подача звукового сигнала
            //    DataBridge.Buzzer.On();

            //    //Запись сообщения в базу данных
            //    DataBridge.AlarmLogging.AddMessage($"Получен штрихкод при остановленной линии", MessageType.Alarm);

            //    //Вывод сообщения в зоне информации
            //    string message = $"Конвейер не запущен, полученный код не будет записан в результат";
            //    var msg = new UserMessage(message, DataBridge.myRed);
            //    DataBridge.MSGBOX.Add(msg);

            //    //Вызов окна
            //    customMessageBox mb = new customMessageBox("Ошибка", "Подтвердите удаление продукта с конвейера!");
            //    mb.Owner = DataBridge.MainScreen;
            //    mb.ShowDialog();


            //    //Выход из метода
            //    return;

            //}

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

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Вывод сообщения в зоне информации
                string message = $"Посторонний код (не является КМ)";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowExtraneousBarcode = new windowExtraneousBarcode(msg);
                windowExtraneousBarcode.Show();

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

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Вывод сообщения в зоне информации
                string message = $"Посторонний продукт (GTIN не совпадает с заданием)";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowExtraneousBarcode = new windowGtinFault(scaner_gtin, scaner_serialnumber, msg);
                windowExtraneousBarcode.Show();

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

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"Номер продукта {scaner_serialnumber} числится в браке", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Вывод сообщения в окно информации
                string message = $"Номер продукта {scaner_serialnumber} числится в браке";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowProductIsDeffect = new windowProductIsDeffect(scaner_serialnumber, msg);
                windowProductIsDeffect.Show();

                //Выход из метода
                return;
            }


            /*
                Повтор кода запрещен
            */
            //Получение статуса тэга
            //ПОВТОР КОДА
            //остановки линии
            var RepeatEnable = REPEAT_ENABLE.Value;           

            //Флаг, указывающий на то, является ли код повтором
            var IsRepeat = DataBridge.Report.IsRepeat(scaner_serialnumber);


            if (RepeatEnable == false && IsRepeat == true)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"Продукт GTIN {scaner_gtin} номер {scaner_serialnumber} считан повторно", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Вывод сообщения в окно информации
                string message = $"Продукт номер {scaner_serialnumber} считан повторно.";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Добавление повтора в отчет
                DataBridge.Report.AddRepeatProduct(scaner_serialnumber);

                //Вызов окна
                var windowRepeatProduct = new windowRepeatProduct(scaner_gtin, scaner_serialnumber, msg);
                windowRepeatProduct.Show();
                               
                //Выход из метода
                return;
            }

            /*
               Если повтор - увеличиваем счетчик на единицу
           */
            //if (IsRepeat == true)
            //{
            //    uint value = 0;
            //    var result = uint.TryParse(RepeatProductCounter.Status.ToString(), out value);
            //    if (result == true)
            //    {
            //        value++;
            //        RepeatProductCounter.Write(value);
            //    }
            //}
            

            /*
                Если проверка прошла успешно добавляем 
                продукт в результат
            */

            //Добавляем просканированное изделие
            //в коллекцию изделий результата
            DataBridge.Report.AddBox(scaner_serialnumber);
        }

        /// <summary>
        /// Метод для очистки коллекции продукции
        /// находящейся между сканером
        /// и отбраковщиком
        /// </summary>
        public void ClearCollection()
        {
            CLEAR_ITEMS_COLLECTION_CMD.Write(true);
        }

        /// <summary>
        /// Команда для очистки коллекции в ПЛК
        /// </summary>
        public ICommand ClearCollectionCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ClearCollection();
                },
                (obj) => (true));
            }
        }

    }
}
