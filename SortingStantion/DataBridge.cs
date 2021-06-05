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
        /// Указатель на главный экран
        /// </summary>
        public static MainScreen.MainScreen MainScreen;

        /// <summary>
        /// Модель, управляющая пользователями
        /// </summary>
        public static AccesLevelModel MainAccesLevelModel = new AccesLevelModel();

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

        /// <summary>
        /// Экземпляр объекта - ручной сканер
        /// </summary>
        public static Scaner Scaner = new Scaner();

        /// <summary>
        /// Элемент, управляющий статистикой
        /// обработаных коробов
        /// </summary>
        public static BoxEngine BoxEngine = new BoxEngine();

    }
}
