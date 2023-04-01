using System;
using System.Drawing;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleColorChangedEventsArgs : EventArgs
{
    public Vehicle Vehicle { get; }
    public byte Index { get; }
    public Color? NewColor { get; }

    public VehicleColorChangedEventsArgs(Vehicle vehicle, byte index, Color? newColor)
    {
        this.Vehicle = vehicle;
        this.Index = index;
        this.NewColor = newColor;
    }
}
