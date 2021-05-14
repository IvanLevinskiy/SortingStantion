using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S7Communication
{
    public enum ErrorCode
    {
        NoError,
        WrongCPU_Type,
        ConnectionError,
        IPAddressNotAvailable,
        WrongVarFormat = 10,
        WrongNumberReceivedBytes,
        SendData = 20,
        ReadData = 30,
        WriteData = 50
    }
}
