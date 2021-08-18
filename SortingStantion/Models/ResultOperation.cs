﻿using System.Text.Json;
using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Collections.ObjectModel;
using S7Communication;

namespace SortingStantion.Models
{
    /// <summary>
    /// Класс, реализующий результат операций
    /// </summary>
    public class ResultOperation
    {
        /// <summary>
        /// Уникальный идентификатор задания. 
        /// </summary>
        public string ID;

        /// <summary>
        /// Время начала работы с заданием
        /// </summary>
        public string startTime;

        /// <summary>
        /// Время окончания работы с заданием 
        /// </summary>
        public string endTime;

        /// <summary>
        /// Массив, содержащий объекты Operator 
        /// описывающие мастера на линии
        /// </summary>
        public List<UserAuthorizationHistotyItem> operators = new List<UserAuthorizationHistotyItem>();

        /// <summary>
        /// Акцессор для предоставления упрощенного
        /// доступа к экземпляру последнего оператора
        /// </summary>
        public UserAuthorizationHistotyItem LastOperator
        {
            get
            {
                if (operators.Count == 0)
                {
                    return null;
                }

                //Последний оператор
                return operators[operators.Count - 1];
            }
        }

        /// <summary>
        /// Список всех кодов, находящихся в результате
        /// </summary>
        public ObservableCollection<string> AllCodes
        {
            get;
            set;
        }

        /// <summary>
        /// Список всех номеров продуктов 
        /// </summary>
        public List<string> Codes = new List<string>();

        /// <summary>
        /// Массив отбракованных вручную номеров продуктов 
        /// </summary>
        public List<string> defectiveCodes = new List<string>();

        /// <summary>
        /// Массив, содержащий номера продуктов, прошедших сканер повторно.
        /// </summary>
        public List<RepeatPack> repeatPacks = new List<RepeatPack>();

        /// <summary>
        /// Текущее рабочее задание
        /// </summary>
        public WorkAssignment CurrentWorkAssignment
        {
            get;
            set;
        }

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticServer server
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
        /// Счетчик повторов
        /// </summary>
        public S7_DWord QUANTITY_REPEAT_PRODUCTS;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public ResultOperation()
        {
            //Инициализация тэга - повтор продуктов
            QUANTITY_REPEAT_PRODUCTS = (S7_DWord)device.GetTagByAddress("DB1.DBD36-DWORD");

            //Инициализация коллекции всех кодов
            //находящихся в результате
            AllCodes = new ObservableCollection<string>();

            //Подпись на событие по принятию задания
            DataBridge.WorkAssignmentEngine.WorkOrderAcceptanceNotification += (workAssignment) =>
            {
                startTime = DateTime.Now.GetDateTimeFormats()[43];
                CurrentWorkAssignment = workAssignment;
            };

            //Подпись на событие по завершению задания
            DataBridge.WorkAssignmentEngine.WorkOrderCompletionNotification += (workAssignment) =>
            {
                endTime = DateTime.Now.GetDateTimeFormats()[43];
                CurrentWorkAssignment = null;
            };

            //Подпись на событие по авторизации нового пользователя
            DataBridge.MainAccesLevelModel.ChangeUser += (accesslevel, currentuser) =>
            {
                //Если оператор имеется,
                //записываем время когда он вышел из логина
                if (LastOperator != null)
                {
                    LastOperator.endTime = DateTime.Now.GetDateTimeFormats()[43];
                }

                var historyitem = new UserAuthorizationHistotyItem(currentuser);
                operators.Add(historyitem);
            };

            //Загрузка результата из файла
            Load();

        }

        /// <summary>
        /// Метод, указывающий на то,
        /// содержится ли код в браке
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        public bool IsDeffect(string serialnumber)
        {
            foreach (var dc in defectiveCodes)
            {
                if (dc == serialnumber)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Метод, указывающий на то,
        /// содержится ли код в результате
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        public bool AsAResult(string serialnumber)
        {
            foreach (var code in AllCodes)
            {
                if (code == serialnumber)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Метод для проверки повторяется ли продукт или нет
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        public bool IsRepeat(string serialnumber)
        {
            /*
                Проверка кодов в списке кодов
            */
            foreach (var code in Codes)
            {
                if (code == serialnumber)
                {
                    return true;
                }
            }

            /*
                Проверка кодов в списке повторов
            */
            foreach (var repeatPack in repeatPacks)
            {
                if (repeatPack.num == serialnumber)
                {
                    return true;
                }
            }

            //Если не нашли код, возвращаем
            //результат
            return false;
        }

        /// <summary>
        /// Метод для добавления нового
        /// просканированного короба
        /// </summary>
        public void AddBox(string serialnumber)
        {
            /*
                Добавление продукта в список
                продуктов
            */
            AllCodes.Insert(0, serialnumber);

            /*
                Если кодов больше 6
                удаляем самый ранний
            */
            //if (AllCodes.Count > 6)
            //{
            //    AllCodes.RemoveAt(AllCodes.Count - 1);
            //}

            /*
                Объявление локальных переменных
            */
            var msg = string.Empty;
            UserMessage messageItem = null;
                        

            /*
                Если код повторяется 
            */
            if (IsRepeat(serialnumber) == true)
            {
                //Добавление кодов в повторы
                AddRepeatProduct(serialnumber);

                //Вывод сообщений
                msg = $"Считан продукт с серийным номером {serialnumber}. Продукт добавлен в коллекцию кодов повторов";
                messageItem = new Controls.UserMessage(msg, DataBridge.myGreen);
                DataBridge.MSGBOX.Add(messageItem);

                //Увеличение счетчика повторов
                uint repeatCount = (uint)QUANTITY_REPEAT_PRODUCTS.Status;
                repeatCount++;
                QUANTITY_REPEAT_PRODUCTS.Write(repeatCount);

                //Выход из процедуры
                return;
            }

            /*
                Если код не повторяется 
            */
            Codes.Add(serialnumber);

            //Вывод сообщений
            msg = $"Считан продукт с серийным номером {serialnumber}. В результате продуктов: {Codes.Count}";
            messageItem = new Controls.UserMessage(msg, DataBridge.myGreen);
            DataBridge.MSGBOX.Add(messageItem);
        }

        /// <summary>
        /// Метод для добавления кода повтора продукта
        /// </summary>
        public void AddRepeatProduct(string serialnumber)
        {
            foreach (var repeatPack in repeatPacks)
            {
                //Если уже есть код продукта, который
                //следует добавить, инкрементируем счетчик
                if (repeatPack.num == serialnumber)
                {
                    repeatPack.quantity ++;
                    return;
                }
            }

            //Если кода повтора нет, удаляем код
            //из результата
            RemoteCode(serialnumber);

            //Добавляем экземпляр повтора кода
            var repeatPackItem = new RepeatPack()
            {
                num = serialnumber,
                quantity = 1
            };

            repeatPacks.Add(repeatPackItem);
        }

        /// <summary>
        /// Метод для добавления деффекта в результат
        /// </summary>
        /// <param name="serialnumber"></param>
        public void AddDeffect(string serialnumber)
        {
            RemoteCode(serialnumber);
            RemoteCodeFromFullResult(serialnumber);
            defectiveCodes.Add(serialnumber);
        }

        /// <summary>
        /// Метод для удаления кодов из результата
        /// </summary>
        /// <param name="serialnumber"></param>
        void RemoteCode(string serialnumber)
        {
            M0: foreach (var code in Codes)
            {
                if (serialnumber == code)
                {
                    Codes.Remove(code);
                    goto M0;
                }
            }
        }

        void RemoteCodeFromFullResult(string serialnumber)
        {
        M0: foreach (var code in AllCodes)
            {
                if (serialnumber == code)
                {
                    AllCodes.Remove(code);
                    goto M0;
                }
            }
        }

        /// <summary>
        /// Метод для сериализации отчета
        /// </summary>
        /// <returns></returns>
        string Serialize()
        {
            //Если за время работы никто не авторизировался
            //сериализацию результата не производим
            if (LastOperator == null)
            {
                return "";
            }

            //Если время работы последнего оператора
            //уазываем его
            if (string.IsNullOrEmpty(LastOperator.endTime) == true)
            {
                LastOperator.endTime = DateTime.Now.GetDateTimeFormats()[43];
            }
            

            //Создание бэкап файла
            var reportBackupFile = new ReportBackupFile()
            {
                id = this.CurrentWorkAssignment.ID,
                operators = this.operators,
                startTime = this.startTime,
                endTime = DateTime.Now.GetDateTimeFormats()[43],
                defectiveCodes = this.defectiveCodes,
                Packs = this.Codes,
                repeatPacks = this.repeatPacks
            };

            //Сериализация
            return JsonSerializer.Serialize<ReportBackupFile>(reportBackupFile);
        }


        /// <summary>
        /// Метод для сохранения
        /// результата в файл
        /// </summary>
        public void Save()
        {
            if (CurrentWorkAssignment == null)
            {
                return;
            }

            //Получение сериализованного отчета
            string json = Serialize();

            //Если ответ не сериализирован, выходим
            if (string.IsNullOrEmpty(json) == true)
            {
                return;
            }

            //Сохранение файла
            StreamWriter sr = new StreamWriter(@"AppData\Task.json");
            sr.Write(json);
            sr.Close();
        }

        async void Load()
        {
            //Если файла нет, выходим из функции
            if (File.Exists(@"AppData\Task.json") == false)
            {
                return;
            }

            ReportBackupFile reportBackupFile = null;

            //Десериализация задачи из памяти программы
            // чтение данных
            using (FileStream fs = new FileStream(@"AppData\Task.json", FileMode.OpenOrCreate))
            {
                reportBackupFile = await JsonSerializer.DeserializeAsync<ReportBackupFile>(fs);
                fs.Close();

                this.ID = reportBackupFile.id;
                this.operators = reportBackupFile.operators;
                this.repeatPacks = reportBackupFile.repeatPacks;
                this.startTime = reportBackupFile.startTime;
                this.endTime = reportBackupFile.endTime;
                this.defectiveCodes = reportBackupFile.defectiveCodes;
                this.Codes = reportBackupFile.Packs;

            }
        }

        /// <summary>
        /// метод для отправки отчета
        /// </summary>
        public bool SendReport()
        {
            Reset();
            return true;

            //Результат операции отправки
            //отчета
            bool resultoperation = false;

            //Получен ие настройки с данными конечного получателя отчетов
            var setting = DataBridge.SettingsFile.GetSetting("SrvL3UrlReport");

            //Получение получателя отчета
            var endPoint = setting.Value;

            //Формирование http запроса с отчетом
            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(endPoint);
            httpWebRequest.ContentType = "applicaion/json";
            httpWebRequest.Method = "POST";

            //Отправка отчета конечному получателю
            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = Serialize();

                //Если ответ не сериализирован, выходим
                if (string.IsNullOrEmpty(json) == true)
                {
                    return false;
                }

                sw.Write(json);
            }

            //Прием ответа от получателя отчетов
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var sr = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = sr.ReadToEnd();

                //Если в ответа содержится 201
                //возвращаем true
                if (result.Contains("201"))
                {
                    //Сброс текущего результата
                    Reset();

                    resultoperation = true;
                }
            }

            return resultoperation;
        }

        /// <summary>
        /// Метод для сброса результата операций
        /// </summary>
        public void Reset()
        {
            //Удаление файла
            if (File.Exists(@"AppData\Task.json") == true)
            {
                File.Delete(@"AppData\Task.json");
            }

            //Обнуление результата
            this.ID = string.Empty;
            this.operators = new List<UserAuthorizationHistotyItem>();
            this.repeatPacks = new List<RepeatPack>();
            this.startTime = string.Empty;
            this.endTime = string.Empty;
            this.defectiveCodes = new List<string>();
            this.Codes = new List<string>();
            AllCodes.Clear();
        }
    }
}
