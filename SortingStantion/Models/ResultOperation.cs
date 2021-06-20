using SortingStantion.Models;
using System;
using System.Collections.Generic;

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
        public List<string> repeatPacks = new List<string>();

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
                CurrentWorkAssignment = workAssignment;
            };

            //Подпись на событие по завершению задания
            DataBridge.WorkAssignmentEngine.WorkOrderCompletionNotification += (workAssignment) =>
            {
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
        /// Метод для добавления нового
        /// просканированного короба
        /// </summary>
        public void AddBox(string serialnumber)
        {
            //Проверка на то, что код продукта
            //в списке брака
            if (IsDeffect(serialnumber) == true)
            {
                //Здесь надо вызывать окно
                //DataBridge.AlarmsEngine.al_3.Write(true);
                return;
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


    }
}
