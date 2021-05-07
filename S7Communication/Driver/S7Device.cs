using S7Communication.Driver;
using Simatic.Driver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace S7Communication.Driver
{
    public class S7Device : IDisposable
    {
        private Socket _mSocket;

        /// <summary>
        /// IP адрес
        /// </summary>
        public string IP
        {
            get;
            private set;
        }

        /// <summary>
        /// Тип ПЛК
        /// </summary>
        public CpuType CPU
        {
            get;
            private set;
        }

        /// <summary>
        /// Rack
        /// </summary>
        public short Rack
        {
            get;
            private set;
        }

        /// <summary>
        /// Slot
        /// </summary>
        public short Slot
        {
            get;
            private set;
        }

        /// <summary>
        /// Свойство, указывающее доступно ли
        /// устройство Plc
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                bool result;
                using (Ping ping = new Ping())
                {
                    PingReply pingReply;
                    try
                    {
                        pingReply = ping.Send(this.IP);
                    }
                    catch (PingException)
                    {
                        pingReply = null;
                    }
                    result = (pingReply != null && pingReply.Status == IPStatus.Success);
                }
                return result;
            }
        }

        /// <summary>
        /// Свойство, показывает доступно ли
        /// соединение с экземпляром Plc
        /// </summary>
        public bool IsConnected
        {
            get
            {
                bool result;
                try
                {
                    if (this._mSocket == null)
                    {
                        result = false;
                    }
                    else
                    {
                        result = ((!this._mSocket.Poll(1000, SelectMode.SelectRead) || this._mSocket.Available != 0) && this._mSocket.Connected);
                    }
                }
                catch
                {
                    result = false;
                }
                return result;
            }
        }

        /// <summary>
        /// Родительский сервер
        /// </summary>
        public S7Server Server
        {
            get;
            set;
        }

        /// <summary>
        /// Коллекция групп тэгов
        /// </summary>
        public ObservableCollection<S7Group> Groups
        {
            get;
            set;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="cpu"></param>
        /// <param name="ip"></param>
        /// <param name="rack"></param>
        /// <param name="slot"></param>
        public S7Device(CpuType cpu, string ip, short rack, short slot)
        {
            this.IP = ip;
            this.CPU = cpu;
            this.Rack = rack;
            this.Slot = slot;
        }

        /// <summary>
        /// Метод для открытия сессии с ПЛК
        /// </summary>
        /// <returns></returns>
        public ErrorCode Open()
        {
            byte[] buffer = new byte[256];
            
            try
            {
                if (!this.IsAvailable)
                {
                    throw new Exception();
                }
            }
            catch
            {
                ErrorCode result = ErrorCode.IPAddressNotAvailable;
                return result;
            }

            //Создание сокета
            try
            {
                this._mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
                this._mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(this.IP), 102);
                this._mSocket.Connect(remoteEP);
            }
            catch (Exception ex)
            {
                return ErrorCode.ConnectionError;
            }
            try
            {
                byte[] array = new byte[]
                {
                    3,
                    0,
                    0,
                    22,
                    17,
                    224,
                    0,
                    0,
                    0,
                    46,
                    0,
                    193,
                    2,
                    1,
                    0,
                    194,
                    2,
                    3,
                    0,
                    192,
                    1,
                    9
                };

                CpuType cPU = this.CPU;
                if (cPU <= CpuType.S7300)
                {
                    if (cPU == CpuType.S7200)
                    {
                        array[11] = 193;
                        array[12] = 2;
                        array[13] = 16;
                        array[14] = 0;
                        array[15] = 194;
                        array[16] = 2;
                        array[17] = 16;
                        array[18] = 0;
                        goto IL_241;
                    }
                    if (cPU != CpuType.S7300)
                    {
                        goto IL_23A;
                    }
                }
                else
                {
                    if (cPU == CpuType.S7400)
                    {
                        array[11] = 193;
                        array[12] = 2;
                        array[13] = 1;
                        array[14] = 0;
                        array[15] = 194;
                        array[16] = 2;
                        array[17] = 3;
                        array[18] = (byte)(this.Rack * 2 * 16 + this.Slot);
                        goto IL_241;
                    }
                    if (cPU != CpuType.S71200)
                    {
                        if (cPU != CpuType.S71500)
                        {
                            goto IL_23A;
                        }
                        array[11] = 193;
                        array[12] = 2;
                        array[13] = 16;
                        array[14] = 2;
                        array[15] = 194;
                        array[16] = 2;
                        array[17] = 3;
                        array[18] = (byte)(this.Rack * 2 * 16 + this.Slot);
                        goto IL_241;
                    }
                }
                array[11] = 193;
                array[12] = 2;
                array[13] = 1;
                array[14] = 0;
                array[15] = 194;
                array[16] = 2;
                array[17] = 3;
                array[18] = (byte)(this.Rack * 2 * 16 + this.Slot);
                goto IL_241;
            IL_23A:
                ErrorCode result = ErrorCode.WrongCPU_Type;
                return result;
            IL_241:
                this._mSocket.Send(array, 22, SocketFlags.None);
                if (this._mSocket.Receive(buffer, 22, SocketFlags.None) != 22)
                {
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                }
                byte[] buffer2 = new byte[]
                {
                    3,
                    0,
                    0,
                    25,
                    2,
                    240,
                    128,
                    50,
                    1,
                    0,
                    0,
                    255,
                    255,
                    0,
                    8,
                    0,
                    0,
                    240,
                    0,
                    0,
                    3,
                    0,
                    3,
                    1,
                    0
                };
                this._mSocket.Send(buffer2, 25, SocketFlags.None);
                if (this._mSocket.Receive(buffer, 27, SocketFlags.None) != 27)
                {
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                }
            }
            catch (Exception ex2)
            {
                return ErrorCode.ConnectionError;
            }
            return ErrorCode.NoError;
        }

        /// <summary>
        /// Метод для завершения сессии с ПЛК
        /// </summary>
        public void Close()
        {
            if (this._mSocket != null && this._mSocket.Connected)
            {
                this._mSocket.Shutdown(SocketShutdown.Both);
                this._mSocket.Close();
            }
        }

        /// <summary>
        /// Мето для чтения данных из ПЛК
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="count"></param>
        /// <returns>Массив байт, полученных в ответ от ПЛК</returns>
        public byte[] ReadBytes(DataType dataType, int db, int startByteAdr, int count)
        {
            List<byte> list = new List<byte>();
            int num = startByteAdr;
            while (count > 0)
            {
                int num2 = Math.Min(count, 200);
                byte[] array = this.ReadBytesWithASingleRequest(dataType, db, num, num2);
                if (array == null)
                {
                    return list.ToArray();
                }
                list.AddRange(array);
                count -= num2;
                num += num2;
            }
            return list.ToArray();
        }


        /// <summary>
        /// Метод для записи значения переменной
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ErrorCode WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value)
        {
            byte[] array = new byte[513];
            ErrorCode result;
            try
            {
                int num = value.Length;
                int num2 = 35 + value.Length;
                ByteArray byteArray = new ByteArray(num2);
                ByteArray arg_2C_0 = byteArray;
                byte[] expr_28 = new byte[3];
                expr_28[0] = 3;
                arg_2C_0.Add(expr_28);
                byteArray.Add((byte)num2);
                

                byteArray.Add(new byte[] 
                { 
                    2, 240, 128, 50, 1, 0, 0
                });
                byteArray.Add(Word.ToByteArray((ushort)(num - 1)));
                byteArray.Add(new byte[]
                {
                    0,
                    14
                });
                byteArray.Add(Word.ToByteArray((ushort)(num + 4)));
                byteArray.Add(new byte[]
                {
                    5,
                    1,
                    18,
                    10,
                    16,
                    2
                });
                byteArray.Add(Word.ToByteArray((ushort)num));
                byteArray.Add(Word.ToByteArray((ushort)db));
                byteArray.Add((byte)dataType);
                int num3 = (int)((long)(startByteAdr * 8) / 65535L);
                byteArray.Add((byte)num3);
                byteArray.Add(Word.ToByteArray((ushort)(startByteAdr * 8)));
                byteArray.Add(new byte[]
                {
                    0,
                    4
                });
                byteArray.Add(Word.ToByteArray((ushort)(num * 8)));
                byteArray.Add(value);


                this._mSocket.Send(byteArray.array, byteArray.array.Length, SocketFlags.None);
                this._mSocket.Receive(array, 512, SocketFlags.None);
                
                if (array[21] != 255)
                {
                    result = ErrorCode.WrongNumberReceivedBytes;
                    throw new Exception(result.ToString());
                }
                result = ErrorCode.NoError;
            }
            catch (Exception ex)
            {
                result = ErrorCode.WriteData;
            }
            return result;
        }


        private ErrorCode WriteMultipleBytes(List<byte> bytes, int db, int startByteAdr = 0)
        {
            ErrorCode errorCode = ErrorCode.NoError;
            int num = startByteAdr;
            try
            {
                while (bytes.Count > 0)
                {
                    int num2 = Math.Min(bytes.Count, 200);
                    List<byte> range = bytes.ToList<byte>().GetRange(0, num2);
                    errorCode = this.WriteBytes(DataType.DataBlock, db, num, range.ToArray());
                    bytes.RemoveRange(0, num2);
                    num += num2;
                    if (errorCode != ErrorCode.NoError)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                errorCode = ErrorCode.WriteData;
            }
            return errorCode;
        }

        /// <summary>
        /// Метод для чтения заголовочного пакета
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private ByteArray ReadHeaderPackage(int amount = 1)
        {
            ByteArray byteArray = new ByteArray(19);
            ByteArray arg_13_0 = byteArray;
            byte[] expr_0F = new byte[3];
            expr_0F[0] = 3;
            arg_13_0.Add(expr_0F);
            byteArray.Add((byte)(19 + 12 * amount));
            byteArray.Add(new byte[] { 2, 240, 128, 50, 1, 0, 0, 0, 0});
            byteArray.Add(Word.ToByteArray((ushort)(2 + amount * 12)));
            byteArray.Add(new byte[] { 0, 0, 4 });
            byteArray.Add((byte)amount);
            return byteArray;
        }

        /// <summary>
        /// Метод для создания пакета, позволяющего
        /// прочитать данные из ПЛК
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private ByteArray CreateReadDataRequestPackage(DataType dataType, int db, int startByteAdr, int count = 1)
        {
            ByteArray byteArray = new ByteArray(12);
            byteArray.Add(new byte[]
            {
                18,
                10,
                16
            });
            if (dataType == DataType.Counter || dataType == DataType.Timer)
            {
                byteArray.Add((byte)dataType);
            }
            else
            {
                byteArray.Add(2);
            }
            byteArray.Add(Word.ToByteArray((ushort)count));
            byteArray.Add(Word.ToByteArray((ushort)db));
            byteArray.Add((byte)dataType);
            int num = (int)((long)(startByteAdr * 8) / 65535L);
            byteArray.Add((byte)num);
            if (dataType == DataType.Counter || dataType == DataType.Timer)
            {
                byteArray.Add(Word.ToByteArray((ushort)startByteAdr));
            }
            else
            {
                byteArray.Add(Word.ToByteArray((ushort)(startByteAdr * 8)));
            }
            return byteArray;
        }

        /// <summary>
        /// Метод для чтения данных
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private byte[] ReadBytesWithASingleRequest(DataType dataType, int db, int startByteAdr, int count)
        {
            byte[] array = new byte[count];
            byte[] result;
            try
            {
                ByteArray byteArray = new ByteArray(31);
                byteArray.Add(this.ReadHeaderPackage(1));
                byteArray.Add(this.CreateReadDataRequestPackage(dataType, db, startByteAdr, count));
                this._mSocket.Send(byteArray.array, byteArray.array.Length, SocketFlags.None);
                byte[] array2 = new byte[512];
                this._mSocket.Receive(array2, 512, SocketFlags.None);
                if (array2[21] != 255)
                {
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                }
                for (int i = 0; i < count; i++)
                {
                    array[i] = array2[i + 25];
                }
                result = array;
            }
            catch (SocketException ex)
            {
                result = null;
            }
            catch (Exception ex2)
            {
                result = null;
            }
            return result;
        }

        

        /// <summary>
        /// Метод для определения
        /// длины типа данных
        /// </summary>
        /// <param name="varType"></param>
        /// <param name="varCount"></param>
        /// <returns></returns>
        private int VarTypeToByteLength(VarType varType, int varCount = 1)
        {
            switch (varType)
            {
                case VarType.Bit: return varCount;
                case VarType.Byte:
                    if (varCount >= 1)
                    {
                        return varCount;
                    }
                    return 1;

                case VarType.Word:
                case VarType.Int:
                case VarType.Timer:
                case VarType.Counter:
                    return varCount * 2;

                case VarType.DWord:
                case VarType.DInt:
                case VarType.Real:
                    return varCount * 4;

                case VarType.String:
                    return varCount;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Уничтожитель
        /// </summary>
        public void Dispose()
        {
            if (this._mSocket != null && this._mSocket.Connected)
            {
                this._mSocket.Shutdown(SocketShutdown.Both);
                this._mSocket.Close();
            }
        }
    }
}