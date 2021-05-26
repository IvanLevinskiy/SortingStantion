using SortingStantion.Controls;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SortingStantion.Models
{

    /// <summary>
    /// Класс, осуществляющий управление 
    /// сообщениями в информационном окне
    /// </summary>
    public class MSG_ENGINE
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
            grid.Children.Clear();
            grid.Children.Add(msg);
            MESSAGES.Add(msg);
        }

        /// <summary>
        /// Метод для удаления сообщения из 
        /// контейнера
        /// </summary>
        /// <param name="msg"></param>
        public void Remove(MSG msg)
        {
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
