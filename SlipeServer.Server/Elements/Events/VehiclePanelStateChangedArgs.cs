using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehiclePanelStateChangedArgs : EventArgs
{
    public Vehicle Vehicle { get; }
    public VehiclePanel Panel { get; }
    public VehiclePanelState State { get; }

    public VehiclePanelStateChangedArgs(Vehicle vehicle, VehiclePanel panel, VehiclePanelState state)
    {
        this.Vehicle = vehicle;
        this.Panel = panel;
        this.State = state;
    }
}
