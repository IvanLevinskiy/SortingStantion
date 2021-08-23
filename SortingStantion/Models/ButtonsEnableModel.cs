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
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //1. Задание не прислано в комплекс
                //   - АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return true;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - НЕ АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return false;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА
                if (inwork == true && currentUser != null)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "АВТОРИЗАЦИЯ"
        /// </summary>
        public bool BtnAutorizationEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //1. Задание не прислано в комплекс - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return true;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА
                if (inwork == true && currentUser != null)
                {
                    return true;
                }

                //Возврат false
                return false;
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
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //Флаг, указывающий на то, запущен ли конвейер
                var lineisrun = DataBridge.Conveyor.LineIsRun;

                //1. Задание не прислано в комплекс
                //   - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - НЕ АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return false;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА и линия не запущена
                if (inwork == true && currentUser != null && lineisrun == false)
                {
                    return true;
                }

                //Возврат false
                return false;
            }

        }

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "СТАРТ-СТОП" на главном экране
        /// </summary>
        public bool BtnStopEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //Флаг, указывающий на то, запущен ли конвейер
                var lineisrun = DataBridge.Conveyor.LineIsRun;

                //1. Задание не прислано в комплекс
                //   - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - НЕ АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return false;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА и линия запущена
                if (inwork == true && currentUser != null && lineisrun == true)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }

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

        /// <summary>
        /// Флаз, задающий режим доступа к кнопке 
        /// ПРИНЯТЬ ЗАДАНИЕ на главном экране
        /// </summary>
        public bool BtnAcceptTaskEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //1. Задание не прислано в комплекс
                //   - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   - АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }

        /// <summary>
        /// Флаз, задающий режим доступа к кнопке 
        /// ЗАВЕРШИТЬ ЗАДАНИЕ на главном экране
        /// </summary>
        public bool BtnFinishTaskEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //1. Задание не прислано в комплекс - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return true;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА
                if (inwork == true && currentUser != null)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }

        /// <summary>
        /// Флаг, задающий режим доступа к кнопкам 
        /// БРАК СПРАВКА ДОБАВИТЬ на главном экране
        /// </summary>
        public bool BtnToolsEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //1. Задание не прислано в комплекс - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - НЕ АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return false;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА
                if (inwork == true && currentUser != null)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }


        /// <summary>
        /// Конструктор класса
        /// </summary>
        public ButtonsEnableModel()
        {
            DataBridge.LoadComplete += DataBridge_LoadComplete;           
        }

        /// <summary>
        /// Метод, вызываемый по окончании инициализации
        /// приложения
        /// </summary>
        private void DataBridge_LoadComplete()
        {
            //Подпись на событие по изменению пользователя
            DataBridge.MainAccesLevelModel.ChangeUser += (a1, a2) =>
            {
                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnSettingsEnable");

                OnPropertyChanged("BtnAcceptTaskEnable");
                OnPropertyChanged("BtnFinishTaskEnable");

                OnPropertyChanged("BtnToolsEnable");
                

            };

            //Подпись на событие по изменению сотояния статуса
            //линии
            DataBridge.Conveyor.ChangeState += () =>
            {
                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");
            };

            //Подписка на событие по получению нового
            //задания от сервера L3
            DataBridge.WorkAssignmentEngine.NewWorkOrderHasArrivedNotification += (oreder) =>
            {
                OnPropertyChanged("BtnSettingsEnable");
                OnPropertyChanged("BtnAutorizationEnable");

                OnPropertyChanged("BtnAcceptTaskEnable");
                OnPropertyChanged("BtnFinishTaskEnable");

                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnToolsEnable");
            };

            //Подпись на событие по принятию нового задания
            DataBridge.WorkAssignmentEngine.WorkOrderAcceptanceNotification += (o) =>
            {
                OnPropertyChanged("BtnSettingsEnable");
                OnPropertyChanged("BtnAutorizationEnable");

                OnPropertyChanged("BtnAcceptTaskEnable");
                OnPropertyChanged("BtnFinishTaskEnable");

                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnToolsEnable");
            };

            //Подпись на событие по завершению задания
            //и отправке результата
            DataBridge.WorkAssignmentEngine.WorkOrderCompletionNotification += (o) =>
            {
                OnPropertyChanged("BtnSettingsEnable");
                OnPropertyChanged("BtnAutorizationEnable");

                OnPropertyChanged("BtnAcceptTaskEnable");
                OnPropertyChanged("BtnFinishTaskEnable");

                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnToolsEnable");
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
