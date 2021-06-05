using SortingStantion.Controls;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SortingStantion.Models
{

    /// <summary>
    /// Класс, осуществляющий управление 
    /// сообщениями в информационном окне
    /// </summary>
    public class Message_Engine
    {
        /// <summary>
        /// Коллекция всех сообщений
        /// </summary>
        public List<MSG> MESSAGES = new List<MSG>();

        /// <summary>
        /// Экземпляр контейнера, в котором будут
        /// хранится сообщения
        /// </summary>
        public Grid grid;

        /// <summary>
        /// метод для добавления сообщения в контейнер
        /// </summary>
        /// <param name="msg"></param>
        public void Add(MSG msg)
        {
            //Очищаем содержимое месседж бокса
            grid.Children.Clear();

            //Добавляем сообщение в коллекцию
            MESSAGES.Add(msg);

            //Если сообщений больше 3
            //удаляем самое первое
            if (MESSAGES.Count > 3)
            {
                MESSAGES.RemoveAt(0);
            }

            //Ищем сообщение с самым высоким приоритетом
            var highmsg = MESSAGES[0];
            foreach (var item in MESSAGES)
            {
                if (item.Type > highmsg.Type)
                {
                    highmsg = item;
                }
            }

            grid.Children.Add(msg);
            

           
        }

        /// <summary>
        /// Метод для удаления сообщения из 
        /// контейнера
        /// </summary>
        /// <param name="msg"></param>
        public void Remove(MSG msg)
        {
            //Если указателя на msg нет
            //выходим из функции
            if (msg == null)
            {
                return;
            }

            //Очищаем контейнер
            grid.Children.Clear();
            MESSAGES.Remove(msg);

            //Если в истории сообщений есть сообщения
            //выводим их в контейнер
            if (MESSAGES.Count > 0)
            {
                grid.Children.Add(MESSAGES[MESSAGES.Count - 1]);
            }

        }

    }
}
