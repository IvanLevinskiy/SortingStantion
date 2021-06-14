using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingStantion.Models
{
    /// <summary>
    /// класс, описывающий структуру элемента 
    /// истории авторизации пользователей
    /// </summary>
    public class UserAuthorizationHistotyItem
    {
        /// <summary>
        /// Время входа в логин
        /// </summary>
        public DateTime LoginTime
        {
            get;
            set;
        }

        /// <summary>
        /// Время выхода из логина
        /// </summary>
        public DateTime LogioutTime
        {
            get;
            set;
        }

        /// <summary>
        /// Указатель на пользователя
        /// </summary>
        public User User
        {
            get;
            set;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public UserAuthorizationHistotyItem(User user)
        {
            this.User = user;
            LoginTime = DateTime.Now;
        }


    }
}
