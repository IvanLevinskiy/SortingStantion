using System.ComponentModel;

namespace SortingStantion.Models
{
    /// <summary>
    /// Модель, управляющая доступом кнопок
    /// </summary>
    public class ButtonsEnableModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "НАСТРОЙКИ"
        /// </summary>
        public bool BtnSettingsEnable
        {
            get
            {
                return _btnSettingsEnable && DataBridge.MainAccesLevelModel.CurrentUser != null;
            }
            set
            {
                _btnSettingsEnable = value;
                OnPropertyChanged("BtnSettingsEnable");
            }
        }
        bool _btnSettingsEnable = false;

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "АВТОРИЗАЦИЯ"
        /// </summary>
        public bool BtnAutorizationEnable
        {
            get
            {
                return _btnAutorizationEnable;
            }
            set
            {
                _btnAutorizationEnable = value;
                OnPropertyChanged("BtnAutorizationEnable");
            }
        }
        bool _btnAutorizationEnable = false;

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "СТАРТ-СТОП" на главном экране
        /// </summary>
        public bool BtnStartEnable
        {
            get
            {
                return _btnStartEnable && DataBridge.Conveyor.LineIsRun == false;
            }
            set
            {
                _btnStartEnable = value;
                OnPropertyChanged("BtnStartEnable");
            }
        }
        bool _btnStartEnable = false;

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "СТАРТ-СТОП" на главном экране
        /// </summary>
        public bool BtnStopEnable
        {
            get
            {
                return _btnStopEnable && DataBridge.Conveyor.LineIsRun == true;
            }
            set
            {
                _btnStopEnable = value;
                OnPropertyChanged("BtnStopEnable");
            }
        }
        bool _btnStopEnable = false;

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "СТАРТ-СТОП" на экране настроек
        /// </summary>
        public bool BtnStartStopForceEnable
        {
            get
            {
                return _btnStartStopForceEnable;
            }
            set
            {
                _btnStartStopForceEnable = value;
                OnPropertyChanged("BtnStartStopForceEnable");
            }
        }
        bool _btnStartStopForceEnable = false;

        public bool BtnAcceptTaskEnable
        {
            get
            {
                return _btnAcceptTaskEnable && DataBridge.WorkAssignmentEngine.InWork == false;
            }
            set
            {
                _btnAcceptTaskEnable = value;
                OnPropertyChanged("BtnAcceptTaskEnable");
            }
        }
        bool _btnAcceptTaskEnable = false;

        public bool BtnFinishTaskEnable
        {
            get
            {
                return _btnFinishTaskEnable && DataBridge.WorkAssignmentEngine.InWork == true;
            }
            set
            {
                _btnFinishTaskEnable = value;
                OnPropertyChanged("BtnFinishTaskEnable");
            }
        }
        bool _btnFinishTaskEnable = false;


        public ButtonsEnableModel()
        {
            DataBridge.LoadComplete += DataBridge_LoadComplete;           
        }

        private void DataBridge_LoadComplete()
        {
            DataBridge.MainAccesLevelModel.ChangeUser += (a1, a2) =>
            {
                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnSettingsEnable");

            };

            //Подпись на событие по изменению сотояния статуса
            //линии
            DataBridge.Conveyor.ChangeState += () =>
            {
                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");
            };

            //Подпись на событие по принятию нового задания
            DataBridge.WorkAssignmentEngine.WorkOrderAcceptanceNotification += (o) =>
            {
                BtnSettingsEnable = true;

                BtnAcceptTaskEnable = false;
                BtnFinishTaskEnable = true;
            };

            //Подпись на событие по принятию нового задания
            DataBridge.WorkAssignmentEngine.WorkOrderCompletionNotification += (o) =>
            {
                BtnAcceptTaskEnable = false;
                BtnFinishTaskEnable = false;
                //OnPropertyChanged("BtnAcceptTaskEnable");
                //OnPropertyChanged("BtnFinishTaskEnable");
            };
        }


        #region Реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }

        }
        #endregion
    }
}
