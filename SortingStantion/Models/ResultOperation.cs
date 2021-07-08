using System.Text.Json;
using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using System.Collections.Generic;
using System.IO;

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
        public DateTime startTime;

        /// <summary>
        /// Время окончания работы с заданием 
        /// </summary>
        public DateTime endTime;

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
        /// Конструктор класса
        /// </summary>
        public ResultOperation()
        {
            //Подпись на событие по принятию задания
            DataBridge.WorkAssignmentEngine.WorkOrderAcceptanceNotification += (workAssignment) =>
            {
                startTime = DateTime.Now;
                CurrentWorkAssignment = workAssignment;
            };

            //Подпись на событие по завершению задания
            DataBridge.WorkAssignmentEngine.WorkOrderCompletionNotification += (workAssignment) =>
            {
                endTime = DateTime.Now;
                CurrentWorkAssignment = null;
            };

            //Подпись на событие по авторизации нового пользователя
            DataBridge.MainAccesLevelModel.ChangeUser += (accesslevel, currentuser) =>
            {
                //Если оператор имеется,
                //записываем время когда он вышел из логина
                if (LastOperator != null)
                {
                    LastOperator.LogioutTime = DateTime.Now;
                }

                var historyitem = new UserAuthorizationHistotyItem(currentuser);
                operators.Add(historyitem);
            };

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
            foreach (var code in Codes)
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

                //Выход из процедуры
                return;
            }

            /*
                Если код не повторяется 
            */
            Codes.Add(serialnumber);

            //Вывод сообщений
            msg = $"Считан продукт с серийным номером {serialnumber}. В результате {Codes.Count} коробов";
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
        }

        /// <summary>
        /// Метод для добавления деффекта в результат
        /// </summary>
        /// <param name="serialnumber"></param>
        public void AddDeffect(string serialnumber)
        {
            RemoteCode(serialnumber);
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

        string dtFormat(DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss+03:00");
        }

        /// <summary>
        /// Аксессор для получения 
        /// </summary>
        string header
        {
            get
            {
                string result = $"\"id\":\"{ID}\",\n";
                result += $"\"startTime\":\"{dtFormat(startTime)}\",\n";
                result += $"\"endTime\":\"{dtFormat(endTime)}\",\n";

                return result;
            }
        }

        /// <summary>
        /// Метод для формирования отчета в формате JSON
        /// </summary>
        /// <returns></returns>
        public string CreatReport()
        {
            /**
            {
                "id":"108-500056",
                "startTime":"2018-12-18T10:41:48+03:00",
                "endTime":"2018-12-18T10:42:37+03:00",
                "operators":
                [
                {
                    "startTime":"2018-12-18T10:42:06+03:00"
                    "endTime":"2018-12-18T10:42:37+03:00",
                    "id":"101",
                }
                ],
                "defectiveCodes":
                [
                    "Y№BBf2",
                    "2Ft^o9"
                ]

                "Packs":
                [
                    "bF3%hI",
                    "I<GM>j",
                    "P0)8df",
                    "P\".Yj>",
                    ”h6#fR0”,
                    "R_hw\"",
                    "0EDFj+"
                ]

                "repeatPacks":
                [
                {
                     "num": "bF3%hI",
                     "quantity":"2"
                }
                {
                     "num": "hT65?s",
                     "quantity":"1"
                }
                ]
            }
            **/


            string report = "{\n\"id\":" + $"\"{DataBridge.WorkAssignmentEngine.TaskID}\"";

                return report;

        }

        /// <summary>
        /// Метод для сохранения
        /// результата в файл
        /// </summary>
        public void Save()
        {
            if(CurrentWorkAssignment == null)
            { 
            return;
            }

            //Создание бэкап файла
            var reportBackupFile = new ReportBackupFile()
            {
                id = this.CurrentWorkAssignment.ID,
                operators = this.operators,
                startTime = dtFormat(startTime),
                endTime = dtFormat(endTime),
                defectiveCodes = this.defectiveCodes,
                Packs = this.Codes,
                repeatPacks = this.repeatPacks

            };

            //Сериализация
            string json = JsonSerializer.Serialize<ReportBackupFile>(reportBackupFile);

            //Сохранение файла
            StreamWriter sr = new StreamWriter(@"AppData\Task.json");
            sr.Write(json);
            sr.Close();
        }
    }
}
