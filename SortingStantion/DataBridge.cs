using S7Communication;
using SortingStantion.Models;
using SortingStantion.TechnologicalObjects;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SortingStantion
{
    /// <summary>
    /// Класс для храниния и доступа к глобальным объектам
    /// </summary>
    public class DataBridge
    {
        /// <summary>
        /// Красный цвет
        /// </summary>
        public static SolidColorBrush myRed = new SolidColorBrush(Color.FromArgb(0xFF, 0xDB, 0x49, 0x69));

        /// <summary>
        /// Зеленый цвет
        /// </summary>
        public static SolidColorBrush myGreen = new SolidColorBrush(Color.FromArgb(0xFF, 0x6D, 0xC2, 0x7A));

        /// <summary>
        /// Синий цвет
        /// </summary>
        public static SolidColorBrush myBlue = new SolidColorBrush();

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
        public static AuthorizationEngine MainAccesLevelModel = new AuthorizationEngine();

        /// <summary>
        /// Файл с конфигурацией приложения
        /// </summary>
        public static SettingsFile SettingsFile = new SettingsFile(@"AppData\Settings.xml");

        /// <summary>
        /// Элемент, отображающий сообщения пользователя
        /// </summary>
        public static ContentPresenter UserMessagePresenter;

        /// <summary>
        /// Simatic tcp сервер
        /// </summary>
        public static SimaticServer S7Server;

        /// <summary>
        /// Метод для создания Simatic Server
        /// </summary>
        public static void CreateSimaticServer()
        {
            //Создаем экземпляр Simatic Server
            S7Server = new SimaticServer();

            //Получаем ip из настроек
            var ip = SettingsFile.GetValue("PlcIp");

            //Создаем экземпляр устройства
            var s7device = new SimaticDevice(ip, CpuType.S71200, 0, 1);

            //Добавляем устройство в сервер
            S7Server.AddDevice(s7device);

            //Создаем пустую группу для тэгов
            var s7group = new SimaticGroup();

            //Добавляем группу в устройство
            s7device.AddGroup(s7group);
        }

        /// <summary>
        /// Модель, управляющая сообщениями на главном экране
        /// </summary>
        public static Message_Engine MSGBOX = new Message_Engine();

        /// <summary>
        /// Модель управляющая экранами
        /// </summary>
        public static ScreenEngine ScreenEngine = new ScreenEngine();

        /// <summary>
        /// **********   ТЕХНОЛОГИЧЕСКИЕ ОБЪЕКТЫ ***************
        /// </summary>

        /// <summary>
        /// Конвейер - линия
        /// </summary>
        public static Conveyor Conveyor = new Conveyor();

        /// <summary>
        /// Модуль, управляющий рабочими заданиямиРабочее задание
        /// </summary>
        public static WorkAssignmentEngine WorkAssignmentEngine = new WorkAssignmentEngine();

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

        /// <summary>
        /// Элемент разделяющий данные штрихкода
        /// </summary>
        public static DataSpliter DataSpliter = new DataSpliter();

        /// <summary>
        /// Объект, осуществляющий логирование и архивацию
        /// сообщений в базу данных
        /// </summary>
        public static AlarmLogging AlarmLogging = new AlarmLogging();

        /// <summary>
        /// Класс, осуществляющий формирование отчета
        /// </summary>
        public static ResultOperation Report = new ResultOperation();

        /// <summary>
        /// Модель, осуществляющая взаимодействие со звуковым излучателем
        /// </summary>
        public static Buzzer Buzzer = new Buzzer();

    }
}
