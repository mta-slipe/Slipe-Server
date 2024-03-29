﻿namespace SlipeServer.Net.Wrappers;

public enum NetWrapperErrorCode
{
    Success = 0,
    UnableToLoadDll = -1001,
    IncompatibilityCheckUnavailable = -1002,
    IsIncompatible = -1003,
    InterfaceUnavailable = -1004,
    UnableToInit = -1005,
    UnableToStart = -1006
}
