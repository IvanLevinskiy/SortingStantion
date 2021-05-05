using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simatic.Driver
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
