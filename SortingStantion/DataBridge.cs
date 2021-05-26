using S7Communication;
using SortingStantion.Models;
using SortingStantion.TechnologicalObjects;
using SortingStantion.UserAdmin;
using System.Windows.Threading;

namespace SortingStantion
{
    public class DataBridge
    {
        /// <summary>
        /// Dispatcher GUI
        /// </summary>
        public static Dispatcher UIDispatcher;

        /// <summary>
        /// Клавная модель, управляющая пользователями
        /// </summary>
        public static AccesLevelModel MainAccesLevelModel;

        /// <summary>
        /// Сервер
        /// </summary>
        public static SimaticServer server = new SimaticServer("AppData/Plc.xml");

        /// <summary>
        /// Модель, управляющая сообщениями на главном экране
        /// </summary>
        public static MSG_ENGINE MSGBOX = new MSG_ENGINE();

        /// <summary>
        /// Модель для управления экранами
        /// </summary>
        public static ScreenEngine ScreenEngine = new ScreenEngine();

        // **********   ТЕХНОЛОГИЧЕСКИЕ ОБЪЕКТЫ ***************//

        /// <summary>
        /// Конвейер - линия
        /// </summary>
        public static Conveyor Conveyor = new Conveyor();

        /// <summary>
        /// Рабочее задание
        /// </summary>
        public static WorkAssignment WorkAssignment = new WorkAssignment();
    }
}
