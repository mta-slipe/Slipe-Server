using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System.Drawing;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents a vehicle's colors.
/// This contains 4 color values, but some vehicles may use less than that.
/// Unused colors are represented by null, and are treated as black by the client.
/// </summary>
public class Colors(Vehicle vehicle, Color? primary = null, Color? secondary = null, Color? color3 = null, Color? color4 = null)
{
    private Color primary = primary ?? Color.Black;
    public Color Primary
    {
        get => this.primary;
        set
        {
            if (this.primary == value)
                return;

            this.primary = value;
            ColorChanged?.Invoke(vehicle, new VehicleColorChangedEventsArgs(vehicle, 0, value));
        }
    }

    private Color secondary = secondary ?? Color.Black;
    public Color Secondary
    {
        get => this.secondary;
        set
        {
            if (this.secondary == value)
                return;

            this.secondary = value;
            ColorChanged?.Invoke(vehicle, new VehicleColorChangedEventsArgs(vehicle, 1, value));
        }
    }

    private Color? color3 = color3;
    public Color? Color3
    {
        get => this.color3;
        set
        {
            if (this.color3 == value)
                return;

            this.color3 = value;
            ColorChanged?.Invoke(vehicle, new VehicleColorChangedEventsArgs(vehicle, 2, value));
        }
    }

    private Color? color4 = color4;
    public Color? Color4
    {
        get => this.color4;
        set
        {
            if (this.color4 == value)
                return;

            this.color4 = value;
            ColorChanged?.Invoke(vehicle, new VehicleColorChangedEventsArgs(vehicle, 3, value));
        }
    }

    public Color?[] AsArray() => new Color?[] { this.Primary, this.Secondary, this.Color3, this.Color4 };

    public event ElementEventHandler<Vehicle, VehicleColorChangedEventsArgs>? ColorChanged;
}
