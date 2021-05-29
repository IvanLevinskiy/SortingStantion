using S7Communication;
using SortingStantion.Models;
using SortingStantion.TechnologicalObjects;
using SortingStantion.UserAdmin;
using System.Windows.Threading;

namespace SortingStantion
{
    /// <summary>
    /// Класс для храниния и доступа к глобальным объектам
    /// </summary>
    public class DataBridge
    {
        /// <summary>
        /// Dispatcher GUI
        /// </summary>
        public static Dispatcher UIDispatcher;

        /// <summary>
        /// Модель, управляющая пользователями
        /// </summary>
        public static AccesLevelModel MainAccesLevelModel;

        /// <summary>
        /// Simatic tcp сервер
        /// </summary>
        public static SimaticServer server = new SimaticServer("AppData/Plc.xml");

        /// <summary>
        /// Модель, управляющая сообщениями на главном экране
        /// </summary>
        public static Message_Engine MSGBOX = new Message_Engine();

        /// <summary>
        /// Модель управляющая экранами
        /// </summary>
        public static ScreenEngine ScreenEngine = new ScreenEngine();

        /// <summary>
        /// Файл с конфигурацией приложения
        /// </summary>
        public static SettingsFile SettingsFile = new SettingsFile(@"AppData\Settings.xml");


        /// <summary>
        /// **********   ТЕХНОЛОГИЧЕСКИЕ ОБЪЕКТЫ ***************
        /// </summary>

        /// <summary>
        /// Конвейер - линия
        /// </summary>
        public static Conveyor Conveyor = new Conveyor();

        /// <summary>
        /// Рабочее задание
        /// </summary>
        public static WorkAssignment WorkAssignment = new WorkAssignment();

        /// <summary>
        /// Объяект, управляющий обработкой аварий от ПЛК
        /// </summary>
        public static AlarmsEngine AlarmsEngine = new AlarmsEngine();

    }
}
