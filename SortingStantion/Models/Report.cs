using SortingStantion.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Collections.ObjectModel;
using S7Communication;
using System.Threading;
using System.Windows.Media;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Newtonsoft.Json;

namespace SortingStantion.Models
{
    /// <summary>
    /// Класс, реализующий результат операций
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Уникальный идентификатор задания. 
        /// </summary>
        public string ID;

        /// <summary>
        /// Время начала работы с заданием
        /// </summary>
        public string startTime;

        /// <summary>
        /// Время окончания работы с заданием 
        /// </summary>
        public string endTime;

        //Настройки из файла настроек
        string SrvL3Login;
        string rvL3Password;
        string SrvL3UrlReport;

        /// <summary>
        /// Массив, содержащий объекты Operator 
        /// описывающие мастера на линии
        /// </summary>
        public List<UserAuthorizationHistotyItem> operators = new List<UserAuthorizationHistotyItem>();

        /// <summary>
        /// Акцессор для предоставления упрощенного
        /// доступа к экземпляру последнего оператора
        /// </summary>
        public UserAuthorizationHistotyItem LastOperator
        {
            get
            {
                if (operators.Count == 0)
                {
                    return null;
                }

                //Последний оператор
                return operators[operators.Count - 1];
            }
        }

        /// <summary>
        /// Список всех кодов, находящихся в результате
        /// </summary>
        public ObservableCollection<string> AllCodes
        {
            get;
            set;
        }

        /// <summary>
        /// Список всех номеров продуктов 
        /// </summary>
        public List<string> Codes = new List<string>();

        /// <summary>
        /// Массив отбракованных вручную номеров продуктов 
        /// </summary>
        public List<string> defectiveCodes = new List<string>();

        /// <summary>
        /// Массив, содержащий номера продуктов, прошедших сканер повторно.
        /// </summary>
        public List<RepeatPack> repeatPacks = new List<RepeatPack>();

        /// <summary>
        /// Текущее рабочее задание
        /// </summary>
        public WorkAssignment CurrentWorkAssignment
        {
            get;
            set;
        }

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticClient server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Счетчик повторов
        /// </summary>
        public S7_DWord QUANTITY_REPEAT_PRODUCTS;

        /// <summary>
        /// Количество выпущеных продуктов
        /// </summary>
        public S7_DWord QUANTITY_PRODUCTS;

        /// <summary>
        /// Количество выпущеных коробов
        /// </summary>
        public S7_DWord QUANTITY_BOXS;

        /// <summary>
        /// Количество продуктов в коробе
        /// </summary>
        public S7_DWord NUM_PACKS_IN_BOX;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Report()
        {
            //Инициализация тэга - повтор продуктов
            QUANTITY_REPEAT_PRODUCTS = (S7_DWord)device.GetTagByAddress("DB1.DBD36-DWORD");

            QUANTITY_PRODUCTS = (S7_DWord)device.GetTagByAddress("DB1.DBD16-DWORD");

            QUANTITY_BOXS = (S7_DWord)device.GetTagByAddress("DB1.DBD20-DWORD");

            NUM_PACKS_IN_BOX = (S7_DWord)device.GetTagByAddress("DB1.DBD362-DWORD");

            //Загрузка настроек из файла
            SrvL3Login = DataBridge.SettingsFile.GetSetting("SrvL3Login").Value;
            rvL3Password = DataBridge.SettingsFile.GetSetting("SrvL3Pass").Value;
            SrvL3UrlReport = DataBridge.SettingsFile.GetSetting("SrvL3UrlReport").Value;

            //Инициализация коллекции всех кодов
            //находящихся в результате
            AllCodes = new ObservableCollection<string>();

            //Подпись на событие по принятию задания
            DataBridge.WorkAssignmentEngine.WorkOrderAcceptanceNotification += (workAssignment) =>
            {
                //startTime = DateTime.Now.GetDateTimeFormats()[43];
                CurrentWorkAssignment = workAssignment;
            };

            //Подпись на событие по завершению задания
            DataBridge.WorkAssignmentEngine.WorkOrderCompletionNotification += (workAssignment) =>
            {
                endTime = DateTime.Now.GetDateTimeFormats()[43];
                CurrentWorkAssignment = null;
            };

            //Подпись на событие по авторизации нового пользователя
            DataBridge.MainAccesLevelModel.ChangeUser += (accesslevel, currentuser) =>
            {
                //Если задание не в работое - запись в историю 
                //регистрации в отчет не заносим
                var inwork = DataBridge.WorkAssignmentEngine.InWork;
                if (inwork == false)
                {
                    return;
                }               

                //Если зарегистрирован не ОПЕРАТОР[0] или не МАСТЕР[1]
                //сведения о регистрации пользователя в отчет не попадают
                if (accesslevel > 1)
                {
                    return;
                }

                //Если оператор не изменился, в историю авторизации
                //мастера не добавляем
                var lastuser = string.Empty;

                //Если указатель на последнего 
                //пользователя не null
                if (LastOperator != null)
                {
                    //Запоминаем ID последнего пользователя
                    lastuser = LastOperator.id;
                }

                //Если последний оператор не
                //поменялся
                if (currentuser.ID == lastuser)
                {
                    return;
                }

                //Сохраняем время завершения работы
                //предыдущего мастера
                if (LastOperator != null)
                {
                    LastOperator.endTime = DateTime.Now.GetDateTimeFormats()[43];
                } 

                //Создаем запись истории регистрации мастера
                var historyitem = new UserAuthorizationHistotyItem(currentuser);

                //Добавляем запись в журнал
                operators.Add(historyitem);
            };

            //Загрузка результата из файла
            Load();

        }

        /// <summary>
        /// Метод, указывающий на то,
        /// содержится ли код в браке
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        public bool IsDeffect(string serialnumber)
        {
            foreach (var dc in defectiveCodes)
            {
                if (dc == serialnumber)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Метод, указывающий на то,
        /// содержится ли код в результате
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        public bool AsAResult(string serialnumber)
        {
            foreach (var code in Codes)
            {
                if (code == serialnumber)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Метод для проверки повторяется ли продукт или нет
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        public bool IsRepeat(string serialnumber)
        {
            /*
                Проверка кодов в списке кодов
            */
            foreach (var code in Codes)
            {
                if (code == serialnumber)
                {
                    return true;
                }
            }

            /*
                Проверка кодов в списке повторов
            */
            foreach (var repeatPack in repeatPacks)
            {
                if (repeatPack.num == serialnumber)
                {
                    return true;
                }
            }

            //Если не нашли код, возвращаем
            //результат
            return false;
        }

        /// <summary>
        /// Метод для проверки содержится ли
        /// продукт в коллекции повторов
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>

        public bool IsContentsRepeatCollection(string serialnumber)
        {
            /*
                Проверка кодов в списке повторов
            */
            foreach (var repeatPack in repeatPacks)
            {
                if (repeatPack.num == serialnumber)
                {
                    return true;
                }
            }

            //Если не нашли код, возвращаем
            //результат
            return false;
        }

        /// <summary>
        /// Метод для добавления нового
        /// просканированного короба
        /// </summary>
        public void AddBox(string serialnumber)
        {
            /*
                Добавление продукта в список
                продуктов
            */
            AllCodes.Insert(0, serialnumber);

            /*
                Если кодов больше 6
                удаляем самый ранний
            */
            if (AllCodes.Count > 6)
            {
                AllCodes.RemoveAt(AllCodes.Count - 1);
            }

            /*
                Объявление локальных переменных
            */
            var msg = string.Empty;
            UserMessage messageItem = null;
                        

            /*
                Если код повторяется 
            */
            if (IsRepeat(serialnumber) == true)
            {
                //Добавление кодов в повторы
                AddRepeatProduct(serialnumber);

                //Вывод сообщений
                //msg = $"Считан продукт с серийным номером {serialnumber}. Продукт добавлен в коллекцию кодов повторов";
                //messageItem = new Controls.UserMessage(msg, DataBridge.myGreen);
                //DataBridge.MSGBOX.Add(messageItem);

                //Увеличение счетчика повторов
                //AddValue(QUANTITY_REPEAT_PRODUCTS, 1);

                //Выход из процедуры
                return;
            }

            /*
                Если код не повторяется 
            */
            Codes.Add(serialnumber);

            /*
                Увеличение счетчика выпущеных
                продуктов в результате
            */
            AddQuantityProducts( num:1);


            //Вывод сообщений
            msg = $"Считан продукт с серийным номером {serialnumber}. В результате продуктов: {Codes.Count}";
            messageItem = new Controls.UserMessage(msg, DataBridge.myGreen);
            DataBridge.MSGBOX.Add(messageItem);
        }

        /// <summary>
        /// Метод для добавления кода повтора продукта
        /// </summary>
        public void AddRepeatProduct(string serialnumber)
        {
            //Увеличение счетчика повторов
            AddValue(tag: QUANTITY_REPEAT_PRODUCTS, num: 1);

            //Осуществляем поиск повтора из отчета
            foreach (var repeatPack in repeatPacks)
            {
                //Если уже есть код продукта, который
                //следует добавить, инкрементируем счетчик
                if (repeatPack.num == serialnumber)
                {
                    repeatPack.quantity ++;

                    return;
                }
            }

            //Добавляем экземпляр повтора кода
            var repeatPackItem = new RepeatPack()
            {
                num = serialnumber,
                quantity = 1
            };

            repeatPacks.Add(repeatPackItem);
        }

        /// <summary>
        /// Метод для добавления деффекта в результат
        /// </summary>
        /// <param name="serialnumber"></param>
        public void AddDeffect(string serialnumber)
        {
            var  dcounter = RemoteCode(serialnumber);
            RemoteCodeFromFullResult(serialnumber);
            defectiveCodes.Add(serialnumber);

            //Вычитание количества удаленных продуктов
            //из счетчика выпущеных продуктов
            AddQuantityProducts( -1 * dcounter);
        }

        /// <summary>
        /// Метод для удаления кодов из результата
        /// </summary>
        /// <param name="serialnumber"></param>
        int RemoteCode(string serialnumber)
        {
            int count = 0;

            M0: foreach (var code in Codes)
            {
                if (serialnumber == code)
                {
                    count++;

                    Codes.Remove(code);
                    goto M0;
                }
            }

            return count;
        }

        /// <summary>
        /// Метод для удаления кода из полного
        /// результата
        /// </summary>
        /// <param name="serialnumber"></param>
        void RemoteCodeFromFullResult(string serialnumber)
        {
        M0: foreach (var code in AllCodes)
            {
                if (serialnumber == code)
                {
                    AllCodes.Remove(code);
                    goto M0;
                }
            }
        }

        /// <summary>
        /// Метод для добавления значения к количеству
        /// выпущенных продуктов
        /// </summary>
        /// <param name="num"></param>
        void AddQuantityProducts(int num)
        {
            /*
               Увеличение счетчика выпущеных
               продуктов в результате
            */
            int qp = (int)QUANTITY_PRODUCTS.Value;

            //Инкрементируем значение счетчика
            qp += num;

            var num_pacs_in_box = NUM_PACKS_IN_BOX.Value;

            //Если ошибок при преобразовании 
            //не возникло - увеличиваем счетчик продуктов на единицу
            //Запись в ПЛК количества выпущеных продуктов
            QUANTITY_PRODUCTS.Write(qp);
            QUANTITY_PRODUCTS.Value = (uint)qp;

            //Запись в ПЛК количества выпущеных коробов
            if (num_pacs_in_box > 0)
            {
                uint quantityBoxs = (uint)(qp / num_pacs_in_box);
                QUANTITY_BOXS.Write(quantityBoxs);
                QUANTITY_BOXS.Value = (uint)quantityBoxs;
            }
        }

        /// <summary>
        /// Метод для добавления значения тэгу
        /// </summary>
        /// <param name="num"></param>
        void AddValue(S7_DWord tag, int num)
        {
            /*
                Вычисление нового значения тэга
            */
            int value = (int)tag.Value + num;

            //Производим запись нового значения
            //в тэг
            
            var result = tag.Write(value);
            if(result == true)
            {
                tag.Value = (uint)value;
                tag.Status = tag.Value;
                tag.StatusText = tag.Value.ToString();
            }

        }

        /// <summary>
        /// Метод для сериализации отчета
        /// </summary>
        /// <returns></returns>
        string Serialize()
        {
            //Если за время работы никто не авторизировался
            //сериализацию результата не производим
            if (LastOperator == null)
            {
                return "";
            }

            //Если время работы последнего оператора
            //уазываем его
            if (string.IsNullOrEmpty(LastOperator.endTime) == true)
            {
                LastOperator.endTime = DateTime.Now.GetDateTimeFormats()[43];
            }
            

            //Создание бэкап файла
            var reportBackupFile = new ReportBackupFile()
            {
                id = this.CurrentWorkAssignment.ID,
                operators = this.operators,
                startTime = this.startTime,
                endTime = DateTime.Now.GetDateTimeFormats()[43],
                defectiveCodes = this.defectiveCodes,
                Packs = this.Codes,
                repeatPacks = this.repeatPacks
            };

            //var options = new JsonSerializerOptions
            //{
            //    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            //    WriteIndented = true
            //};

            //Сериализация
            return JsonConvert.SerializeObject(reportBackupFile);
        }


        /// <summary>
        /// Метод для сохранения
        /// результата в файл
        /// </summary>
        public void Save()
        {
            if (CurrentWorkAssignment == null)
            {
                return;
            }

            //Получение сериализованного отчета
            string json = Serialize();

            //Если ответ не сериализирован, выходим
            if (string.IsNullOrEmpty(json) == true)
            {
                return;
            }

            //Сохранение файла
            StreamWriter sr = new StreamWriter(@"AppData\Task.json");
            sr.Write(json);
            sr.Close();
        }

        /// <summary>
        /// Метод для сохранения
        /// результата в файл в определенную папку
        /// </summary>
        public void Save(string dirName)
        {
            if (CurrentWorkAssignment == null)
            {
                return;
            }

            //Получение сериализованного отчета
            var content = Serialize();

            //Если ответ не сериализирован, выходим
            if (string.IsNullOrEmpty(content) == true)
            {
                return;
            }

            //Если директория не создана
            //создаем её
            if (Directory.Exists(dirName) == false)
            {
                Directory.CreateDirectory(dirName);
            }

            //Получение имени файла
            var filename = $@"{dirName}\{DataBridge.WorkAssignmentEngine.TaskID}.txt";

            //Сохранение файла
            StreamWriter sr = new StreamWriter($@"{filename}");
            sr.Write(content);
            sr.Close();
        }

        async void Load()
        {
            //Если файла нет, выходим из функции
            if (File.Exists(@"AppData\Task.json") == false)
            {
                return;
            }

            ReportBackupFile reportBackupFile = null;

            //Десериализация задачи из памяти программы
            // чтение данных
            using (StreamReader fs = new StreamReader(@"AppData\Task.json"))
            {
                var text = fs.ReadToEnd();
                reportBackupFile = JsonConvert.DeserializeObject<ReportBackupFile>(text);
                fs.Close();

                this.ID = reportBackupFile.id;
                this.operators = reportBackupFile.operators;
                this.repeatPacks = reportBackupFile.repeatPacks;
                this.startTime = reportBackupFile.startTime;
                this.endTime = reportBackupFile.endTime;
                this.defectiveCodes = reportBackupFile.defectiveCodes;
                this.Codes = reportBackupFile.Packs;

            }
        }

        /// <summary>
        /// метод для отправки отчета
        /// Выполняется в отдельном потоке
        /// </summary>
        public void SendReport()
        {
            //Thread thread = new Thread(SendReportToL3);
            //thread.IsBackground = true;
            //thread.Start();
            SendReportToL3();
        }

        private string GetToken(string url, string username, string password, string tenancyName = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var input = "{\"usernameOrEmailAddress\":\"" + username + "\"," +
                            "\"password\":\"" + password + "\"}";

                if (tenancyName != null)
                {
                    input = input.TrimEnd('}') + "," +
                            "\"tenancyName\":\"" + tenancyName + "\"}";
                }

                streamWriter.Write(input);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string response;

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            // Crude way
            var entries = response.TrimStart('{').TrimEnd('}').Replace("\"", String.Empty).Split(',');

            foreach (var entry in entries)
            {
                if (entry.Split(':')[0] == "result")
                {
                    return entry.Split(':')[1];
                }
            }

            return null;
        }

        /// <summary>
        /// Метод для отправки результата на L3
        /// </summary>
        void SendReportToL3()
        {
            var username = SrvL3Login;
            var password = rvL3Password;

            //Получение получателя отчета
            var endPoint = SrvL3UrlReport;

            //Формирование http запроса с отчетом
            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(endPoint);
            httpWebRequest.ContentType = "applicaion/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.PreAuthenticate = true;

            string authorisation = string.Format("{0}:{1}", username,  password);
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorisation));
            string header = string.Format("{0} {1}", "Basic", encoded);
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = header;


            try
            {
                //Отправка отчета конечному получателю
                using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = Serialize();

                    //Если ответ не сериализирован, выходим
                    if (string.IsNullOrEmpty(json) == true)
                    {
                        return;
                    }

                    //Запись данных в поток вывода
                    sw.Write(json);
                }

                //Прием ответа от получателя отчетов
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    //Если в ответа содержится 201
                    //возвращаем true
                    if (httpResponse.StatusCode == HttpStatusCode.Created)
                    {
                        //Сброс текущего результата
                        ClearResult();
                    }
                }
            }
            catch (Exception ex)
            {
                //В случае невозможности отправить
                //отчет на L3 - созраняем его в папке "Report"
                this.Save("Report");

                //Вывод сообщения об отправке отчета
                ShowMessage("Ошибка отправки отчета", DataBridge.myRed);
            }


        }

        /// <summary>
        /// Метод для отображения сообщения в зоне информации
        /// </summary>
        /// <param name="message"></param>
        void ShowMessage(string message, Brush color)
        {
            Action action = () =>
            {
                var msgitem = new UserMessage(message, color);
                DataBridge.MSGBOX.Add(msgitem);
            };
            DataBridge.UIDispatcher.Invoke(action);
        }

        /// <summary>
        /// Метод для очистки результата операций
        /// </summary>
        public void ClearResult()
        {
            //Удаление файла
            if (File.Exists(@"AppData\Task.json") == true)
            {
                File.Delete(@"AppData\Task.json");
            }

            //Обнуление результата
            this.ID = string.Empty;
            this.operators = new List<UserAuthorizationHistotyItem>();
            this.repeatPacks = new List<RepeatPack>();
            this.startTime = string.Empty;
            this.endTime = string.Empty;
            this.defectiveCodes = new List<string>();
            this.Codes.Clear();
            AllCodes.Clear();
        }
    }
}
