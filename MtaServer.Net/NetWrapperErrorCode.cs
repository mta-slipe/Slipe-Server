using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Net
{
    public enum NetWrapperErrorCode
    {
        Success = 0,
        UnableToLoadDll = 1001,
        IncompatibilityCheckUnavailable = 1002,
        IsIncompatible = 1003,
        InterfaceUnavailable = 1004,
    }
}
