using System.Windows.Controls;

namespace SortingStantion.Controls
{
    public class rButton : RadioButton
    {

        /// <summary>
        /// Минимальный уровень доступа
        /// </summary>
        public int MinAccesLevel
        {
            get
            {
                return minAccesLevel;
            }
            set
            {
                minAccesLevel = value;
                this.IsEnabled = CheckAccesLevel((int)DataBridge.MainAccesLevelModel.CurrentUser.AccesLevel);
            }
        }
        int minAccesLevel = 1;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public rButton()
        {
            //DataBridge.MainAccesLevelModel.ChangeUser += Users_ChangeUser;
           // this.IsEnabled = CheckAccesLevel((int)DataBridge.MainAccesLevelModel.CurrentUser.AccesLevel);
        }

        /// <summary>
        /// Собылие при изменении пользователя
        /// </summary>
        /// <param name="accesslevel"></param>
        private void Users_ChangeUser(int accesslevel)
        {
            this.IsEnabled = CheckAccesLevel(accesslevel);
        }

        /// <summary>
        /// Проверка на минимальный уровень доступа
        /// </summary>
        /// <param name="accesslevel"></param>
        /// <returns></returns>
        bool CheckAccesLevel(int accesslevel)
        {
            return accesslevel >= MinAccesLevel;
        }

    }
}
