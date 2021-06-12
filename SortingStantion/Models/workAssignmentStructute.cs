using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SortingStantion.Models
{
    /*
    {
      "id": "f8453afd-de4e-11e7-8110-000c73101135117",
      "gtin": "04604567890126",
      "lotNo": "Series 2",
      "lineNum": "2",
      "numРacksInBox": 4,
      "productName": "Моцарелла 300 гр.",
      "numPacksInSeries": 10000,
    }
    */


    /// <summary>
    /// Класс - структура для десериализации
    /// рабочего задания
    /// </summary>
    public class workAssignmentStructure
    {
        /// <summary>
        /// Уникальный идентификатор задания.
        /// </summary>
        [JsonProperty("id")]
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// Номер GTIN
        /// </summary>
        [JsonProperty("gtin")]
        public string gtin
        {
            get;
            set;
        }

        /// <summary>
        /// Номер производственной серии, до 20 символов
        /// </summary>
        [JsonProperty("lotNo")]
        public string lotNo
        {
            get;
            set;
        }

        /// <summary>
        /// Номер линии для, которой сформировано это задание.
        /// </summary>
        [JsonProperty("lineNum")]
        public string lineNum
        {
            get;
            set;
        }

        /// <summary>
        /// Кол-во продуктов в коробе
        /// </summary>
        [JsonProperty("numРacksInBox")]
        public string numРacksInBox
        {
            get;
            set;
        }

        /// <summary>
        /// Наименование продукта
        /// </summary>
        [JsonProperty("productName")]
        public string productName
        {
            get;
            set;
        }

        /// <summary>
        /// Ожидаемое количество продуктов в серии (определяется по заданию на производство серии) 
        /// </summary>
        [JsonProperty("numPacksInSeries")]
        public string numPacksInSeries
        {
            get;
            set;
        }
    }
}
