using System;
using System.Drawing;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleColorChangedEventsArgs(Vehicle vehicle, byte index, Color newColor) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
    public byte Index { get; } = index;
    public Color NewColor { get; } = newColor;
}
