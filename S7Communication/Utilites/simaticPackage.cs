using System.Collections.Generic;
using System.Linq;

namespace S7Communication
{

    /// <summary>
    /// Класс, реализующий сущность
    /// пакета тэгов, подлежащих опросу за один
    /// раз
    /// 
    /// У тэгов, входящих в пакет
    /// должна быть общая область опрашиваемой памяти
    /// в ПЛК (память меркеров, входов, дб), одна и
    /// та же ДБ
    /// 
    /// </summary>
    public class simaticPackage
    {

        /// <summary>
        /// Тип данных в области ПЛК
        /// </summary>
        MemmoryArea DataType
        {
            get;
            set;
        }

        /// <summary>
        /// Порядковый номер ДБ
        /// </summary>
        public int DBNumber
        {
            get;
            set;
        }


        /// <summary>
        /// Указатель на устройство
        /// </summary>
        public List<simaticTagBase> Tags
        {
            get;
            set;
        }

        /// <summary>
        /// Количество байт
        /// </summary>
        public int Lenght
        {
            get;
            set;
        }

        /// <summary>
        /// Начальный байт
        /// </summary>
        public int StartByte
        {
            get;
            set;
        }

        /// <summary>
        /// Акцессор для облегченого 
        /// доступа к первому тэгу
        /// </summary>
        simaticTagBase FirstTag
        {
            get
            {
                if (Tags.Count == 0)
                {
                    return null;
                }

                return Tags[0];
            }
        }

        /// <summary>
        /// Акцессор для облегченого 
        /// доступа к последнему тэгу
        /// </summary>
        simaticTagBase LastTag
        {
            get
            {
                if (Tags.Count == 0)
                {
                    return null;
                }

                return Tags[Tags.Count - 1];
            }
        }

        /// <summary>
        /// Указатель на устройство
        /// </summary>
        SimaticDevice PointerDevice;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="device"></param>
        public simaticPackage(List<simaticTagBase> tags, SimaticDevice device)
        {
            this.Tags = new List<simaticTagBase>();
            this.PointerDevice = device;

            //Максимальное количество интераций
            int maxinteration = tags.Count;

            //Проход по тэгам и добавление
            //тэгов, удовлетворяющих
            //условиям объединения

            for (int i = 0; i < maxinteration; i++)
            {
                //В случае отсутсвия тэгов
                //выход из цикла
                if (tags.Count == 0)
                {
                    break;
                }

                //Текущий тэг перечисления
                var tag = tags[i];

                //Если коллекция тэгов пустая
                //инициализируем поля
                if (FirstTag == null)
                {
                    DataType = tag.DataType;
                    DBNumber = tag.DBNumber;
                    
                    //Добавление тэга в колекцию
                    //экземпляра
                    Tags.Add(tag);

                    //Удаление тэга их локальной
                    //коллекции устройства
                    tags.Remove(tag);

                    //Поправка на то, что тэг из коллекции удален
                    i--;
                    maxinteration--;

                    //Переход к слежующей интерации
                    continue;
                }

                //Проверка на то, подходит текущий тэг 
                //пакету тэгов по критериям (область памяти, дб, ...)
                if (AddressSpaceIsValid(tag) == true)
                {
                    //Добавление тэга в колекцию
                    //экземпляра
                    Tags.Add(tag);

                    //Удаление тэга их локальной
                    //коллекции устройства
                    tags.Remove(tag);

                    //Поправка на то, что тэг из коллекции удален
                    i--;
                    maxinteration--;
                }
            }

            //Сортировка тэгов по первому байту
            Tags = Tags.OrderBy(o => o.StartByteAddress).ToList();

            //Определяем первый байт в запросе
            StartByte = FirstTag.StartByteAddress;

            //Определение количество байт 
            //за один запрос
            Lenght = LastTag.StartByteAddress + LastTag.Lenght - StartByte;
        }

        /// <summary>
        /// Проверка на совпадеение
        /// адресного пространства
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool AddressSpaceIsValid(simaticTagBase tag)
        {
            bool temp = false;

            temp = (DataType == tag.DataType);
            temp &= (DBNumber == tag.DBNumber);
            temp &= (Tags[0] != tag);

            return temp;

        }


        /// <summary>
        /// Метод для чтения тэгов
        /// </summary>
        public void Read()
        {
            var ReportFromDevice = PointerDevice.ReadBytesWithASingleRequest(DataType, DBNumber, StartByte, Lenght);


            //Заполняем тэги данными
            foreach (var tag in Tags)
            {
                tag.BuildStatus(ReportFromDevice, StartByte);
            }
        }

    }
}
