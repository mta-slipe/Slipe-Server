using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Packets.Enums
{
    public enum VehicleInOutActionReturns
    {
        RequestInConfirmed,
        NotifyInReturn,
        NotifyInAbortReturn,
        RequestOutConfirmed,
        NotifyOutReturn,
        NotifyOutAbortReturn,
        NotifyFellOffReturn,
        RequestJackConfirmed,
        NotifyJackReturn,
        VehicleAttemptFailed,
    }
}