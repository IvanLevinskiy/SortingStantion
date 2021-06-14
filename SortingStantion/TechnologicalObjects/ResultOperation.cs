using SortingStantion.Models;
using System;
using System.Collections.Generic;

namespace SortingStantion.TechnologicalObjects
{
    /// <summary>
    /// Класс, реализующий результат операций
    /// </summary>
    public class ResultOperation
    {
        /// <summary>
        /// Уникальный идентификатор задания. 
        /// </summary>
        string ID;

        /// <summary>
        /// Время начала работы с заданием
        /// </summary>
        DateTime startTime;

        /// <summary>
        /// Время окончания работы с заданием 
        /// </summary>
        DateTime endTime;

        /// <summary>
        /// Массив, содержащий объекты Operator 
        /// описывающие мастера на линии
        /// </summary>
        List<UserAuthorizationHistotyItem> operators = new List<UserAuthorizationHistotyItem>();

        /// <summary>
        /// Акцессор для предоставления упрощенного
        /// доступа к экземпляру последнего оператора
        /// </summary>
        UserAuthorizationHistotyItem LastOperator
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
        List<string> Codes = new List<string>();

        /// <summary>
        /// Массив отбракованных вручную номеров продуктов 
        /// </summary>
        List<string> defectiveCodes = new List<string>();

        /// <summary>
        /// Массив, содержащий номера продуктов, прошедших сканер повторно.
        /// </summary>
        List<string> repeatPacks = new List<string>();

        /// <summary>
        /// Текущее рабочее задание
        /// </summary>
        WorkAssignment CurrentWorkAssignment
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
        /// Метод для проверки того, находится 
        /// продукт в браке или нет
        /// </summary>
        /// <returns></returns>
        bool IsDeffect(string serialnumber)
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
        /// Метод для проверки повторяется ли продукт или нет
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        bool IsRepeat(string serialnumber)
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

    }
}
