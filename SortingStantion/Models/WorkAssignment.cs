using Newtonsoft.Json;
using System.ComponentModel;

namespace SortingStantion.Models
{
    /// <summary>
    /// Класс, описывающий рабочее задание
    /// </summary>
    public class WorkAssignment : INotifyPropertyChanged
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
        public int numРacksInBox
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
        public int numPacksInSeries
        {
            get;
            set;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public WorkAssignment()
        {

        }




        #region Реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }

        }
        #endregion

    }
}
