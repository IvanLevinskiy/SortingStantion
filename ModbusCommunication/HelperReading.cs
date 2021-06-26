using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Communication
{
    /// <summary>
    /// Класс, реализующий помошника для опроса
    /// к устройству
    /// </summary>
    public class HelperReading
    {

        /// <summary>
        /// Указатель на регистры,
        /// которые следует опросить
        /// </summary>
        public List<mbRegisterBase> Registers
        {
            get;
            set;
        }

        /// <summary>
        /// Количество регистров
        /// </summary>
        public int Count
        {
            get;
            set;
        }

        /// <summary>
        /// Начальный регистр опроса
        /// </summary>
        public int StartRegistrID
        {
            get;
            set;
        }

        DateTime TimeStart;

        /// <summary>
        /// Указатель на устройство
        /// </summary>
        mbDevice PointerDevice;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="device"></param>
        public HelperReading(mbRegisterBase[] registers, mbDevice device)
        {
            //Получаем указатель на устройство
            this.PointerDevice = device;

            //Запоминаем время
            TimeStart = DateTime.Now;

            //Инициализируем коллекцию регистров
            Registers = new List<mbRegisterBase>();

            //Переводим массив регистров в список
            foreach (var register in registers)
            {
                Registers.Add(register);
            }

            WriteLog($"{DateTime.Now.ToString("HH.mm:ss.fff")} DEVICE: {device.IP} Lenght: {registers.Length} Time:{device.CyclicTime}\n");

            //Сортируем регистры по адресу
            Registers = Registers.OrderBy(o => o.Address).ToList();
        }

        /// <summary>
        /// Коллекция байт - ответ от устройства
        /// </summary>
        List<byte> ReportFromDevice = new List<byte>();

        /// <summary>
        /// Список регистров, включенных 
        /// в запрос для чтения
        /// </summary>
        List<mbRegisterBase> ReadingRegistrs = new List<mbRegisterBase>();

        /// <summary>
        /// Метод для выполнения запроса
        /// </summary>
        /// <returns>Выполнен ли запрос</returns>
        public bool Execute()
        {
            //Если нет регистров для опроса
            //покидаем метод
            if (Registers.Count <= 0)
            {
                return false;
            }

            //Получаем первый регистр для опроса
            StartRegistrID = Registers[0].Address;

            //Добавляем регистры в соответствии с адресами, которые необходимо
            //прочитать
            int temp_address = StartRegistrID;

            //Добавляем первый регистр в коллекцию регистров,
            //которые необходимо опросить
            //в текущей сессии
            Count = Registers[0].GetEcvivalentRegistersCount();
            ReadingRegistrs.Add(Registers[0]);
            Registers.Remove(Registers[0]);

            //Счетчик байтов
            int Bytecount = ReadingRegistrs[0].Lenght;

            //Собираем запрос
            foreach (var registr in Registers)
            {
                //Если количество байт данных
                //больше 256, остальные байты оставляем
                //для следующего запроса
                if (Bytecount + registr.Lenght >= 252)
                {
                    break;
                }

                //Получаем адрес текущего регистра
                var current_adress = registr.Address;

                //Получаем адрес последнего регистра
                var last_address = ReadingRegistrs[ReadingRegistrs.Count - 1].GetEndAddress();

                //Сравниваем адреса
                if (last_address == current_adress - 1)
                {
                    ReadingRegistrs.Add(registr);
                    Bytecount += registr.Lenght;
                    Count += registr.GetEcvivalentRegistersCount();
                    continue;
                }

                //Если адреса предыдущего регистра
                //не попорядке, выполняем запрос
                //оставив непопадающие в запрос регистры для
                //следующей сессии
                break;
            }

            //Удаляем из коллекции регистры, которые подлежат опросу
            //в текущей сессии
            foreach (var reg in ReadingRegistrs)
            {
                Registers.Remove(reg);
                WriteLog($"register: {reg.Address} Lenght: {reg.Lenght}\n");
            }

            //Запись данных в лог
            WriteLog($"\n\n");

            //Открываем сессию с ПЛК и читаем данные
            ReportFromDevice = PointerDevice.ReadHoldingRegisters((ushort)StartRegistrID, (ushort)Count);

            //Если данных нет, сбрасываем флаг тогого, что устройство
            //доступно в сети
            if (ReportFromDevice.Count == 0)
            {
                PointerDevice.ReconnectRequest = true;
            }
            

            //Заполняем выбранные регистры данными
            foreach (var registr in ReadingRegistrs)
            {
                //Построение статуса регистра
                registr.BuildStatus(ReportFromDevice);
            }

            //Очищаем коллекцию прочитанных регистров
            ReadingRegistrs.Clear();

            //Если в списках регистров есть
            //неопрошенные регистры, возвращаем true
            var r = Registers.Count > 0;

            if (r == false)
            { 
                //Вычисляем время, за которое было опрошено устройство
                PointerDevice.CyclicTime = DateTime.Now - TimeStart;
            }

            return r;
        }

        /// <summary>
        /// Метод для заполнения коллекции
        /// пустыми регистрами, для того, чтоб 
        /// сократить количество инртераций
        /// </summary>
        List<mbRegisterBase> FillingRegisterArray(List<mbRegisterBase> list)
        {
            List<mbRegisterBase> newarray = new List<mbRegisterBase>();

            var lastregister = list[0];

            for (int i = 1; i < list.Count; i ++)
            {
                //Разница
                int dif = list[i].Address - lastregister.GetEndAddress();
                lastregister = list[i];
                newarray.Add(list[i]);

                if (dif > 1 && dif < 10)
                {
                    for (int j = 0; j < dif; j++)
                    {
                        var Register = new mbSingleRegister(lastregister.GetEndAddress() + 1, Types.Ushort);
                        lastregister = Register;
                        newarray.Add(Register);
                    }
                }
            }

            return newarray;
        }

        /// <summary>
        /// Запись данных в лог
        /// </summary>
        /// <param name="log"></param>
        void WriteLog(string log)
        {
            return;

            FileStream fs = new FileStream($"log_{PointerDevice.IP}_{PointerDevice.Port}.txt", FileMode.Append);
            var bts = Encoding.UTF8.GetBytes(log);
            fs.Write(bts, 0, bts.Length);
            fs.Close();
        }
    }
}
