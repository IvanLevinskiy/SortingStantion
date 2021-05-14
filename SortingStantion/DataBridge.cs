using S7Communication;
using SortingStantion.UserAdmin;

namespace SortingStantion
{
    public class DataBridge
    {
        /// <summary>
        /// Клавная модель, управляющая пользователями
        /// </summary>
        public static AccesLevelModel MainAccesLevelModel;

        /// <summary>
        /// Сервер
        /// </summary>
        public static SimaticServer server = new SimaticServer();
    }
}
