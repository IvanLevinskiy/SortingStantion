using System.Collections.Generic;

namespace SortingStantion.Models
{
    /// <summary>
    /// Класс - разделяющий данные
    /// на поля (GTIN, SN, ...)
    /// </summary>
    public class DataSpliter
    {
        /// <summary>
        /// Флаг, указывающий на валидность
        /// принятых данных
        /// </summary>
        public bool IsValid
        {
            get;
            set;
        }

        /// <summary>
        /// GTIN
        /// </summary>
        public string GTIN
        {
            get;
            set;
        }

        /// <summary>
        /// Серийный номер
        /// </summary>
        public string SerialNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Поля в строке
        /// </summary>
        List<DataField> fields;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public DataSpliter()
        {
            //Получение полей из 
            //файла конфигурации
            GetDataFieldList();
        }

        /// <summary>
        /// Метод для получения GTIN из принятых данных
        /// </summary>
        /// <returns></returns>
        public string GetGTIN()
        {
            //Ищем поле с префиксом 01 - это gtin
            foreach (var field in fields)
            {
                if (field.Preficks == "01")
                {
                    return field.Data;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Метод для получения серийного номера
        /// из принятых данных
        /// </summary>
        /// <returns></returns>
        public string GetSerialNumber()
        {
            //Ищем поле с префиксом 01 - это gtin
            foreach (var field in fields)
            {
                if (field.Preficks == "21")
                {
                    return field.Data;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Метод для разделения входящей
        /// строки по полям
        /// </summary>
        /// <param name="data"></param>
        public void Split(ref string data)
        {
            //Установка по умолчанию валидности в true
            IsValid = true;

            //Проверка всех полей и разделение данных
            foreach (var field in fields)
            {
                IsValid &= field.Check(ref data);
            }
        }

        /// <summary>
        /// Метод для получения полей из файла
        /// </summary>
        void GetDataFieldList()
        {
            //Инициализация коллекции полей
            fields = new List<DataField>();

            //Получение коллекции полей
            for (int i = 1; i <= 100; i++)
            {
                //Имя настройки с префиксом
                string PreficksName = $"{i}fieldPreficks";

                //Имя настройки с длиной
                string LengthName = $"{i}fieldLength";

                //Получение из файла конфигурации префикса поля
                var PreficksSetting = DataBridge.SettingsFile.GetSetting(PreficksName);

                //Получение из файла конфигурации длины поля
                var LengthSetting = DataBridge.SettingsFile.GetSetting(LengthName);

                //Если одно из полей пустое, завершаем создание списка полей
                if (PreficksSetting == null || LengthSetting == null)
                {
                    break;
                }

                //Создаем поле
                var field = new DataField(PreficksSetting.Value, LengthSetting.Value);
                fields.Add(field);
            }
        }

    }
}
