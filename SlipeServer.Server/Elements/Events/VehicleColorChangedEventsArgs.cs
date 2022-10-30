using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlipeServer.Server.Elements.Events;

public class VehicleColorChangedEventsArgs : EventArgs
{
    public Vehicle Vehicle { get; set; }
    public byte Index { get; set; }
    public Color NewColor { get; set; }

    public VehicleColorChangedEventsArgs(Vehicle vehicle, byte index, Color newColor)
    {
        this.Vehicle = vehicle;
        this.Index = index;
        this.NewColor = newColor;
    }
}
