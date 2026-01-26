using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehiclePanelStateChangedArgs(Vehicle vehicle, VehiclePanel panel, VehiclePanelState state) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
    public VehiclePanel Panel { get; } = panel;
    public VehiclePanelState State { get; } = state;
}
