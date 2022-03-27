using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Packets.Enums;

public enum VehicleInOutAction
{
    RequestIn,
    NotifyIn,
    NotifyAbortIn,
    RequestOut,
    NotifyOut,
    NotifyOutAbort,
    NotifyJack,
    NotifyJackAbort,
    NotifyFellOff,
}
