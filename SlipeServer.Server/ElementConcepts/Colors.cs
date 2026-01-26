using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System.Drawing;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents a vehicle's colors.
/// This contains 4 color values, but some vehicles may use less than that.
/// </summary>
public class Colors(Vehicle vehicle, Color? primary = null, Color? secondary = null, Color? color3 = null, Color? color4 = null)
{
    private Color primary = primary ?? Color.White;
    public Color Primary
    {
        get => this.primary;
        set
        {
            var args = new VehicleColorChangedEventsArgs(vehicle, 0, value);
            this.primary = value;
            ColorChanged?.Invoke(vehicle, args);
        }
    }

    private Color secondary = secondary ?? Color.White;
    public Color Secondary
    {
        get => this.secondary;
        set
        {
            var args = new VehicleColorChangedEventsArgs(vehicle, 1, value);
            this.secondary = value;
            ColorChanged?.Invoke(vehicle, args);
        }
    }

    private Color color3 = color3 ?? Color.White;
    public Color Color3
    {
        get => this.color3;
        set
        {
            var args = new VehicleColorChangedEventsArgs(vehicle, 2, value);
            this.color3 = value;
            ColorChanged?.Invoke(vehicle, args);
        }
    }

    private Color color4 = color4 ?? Color.White;
    public Color Color4
    {
        get => this.color4;
        set
        {
            var args = new VehicleColorChangedEventsArgs(vehicle, 3, value);
            this.color4 = value;
            ColorChanged?.Invoke(vehicle, args);
        }
    }

    public Color[] AsArray() => new Color[] { this.Primary, this.Secondary, this.Color3, this.Color4 };

    public event ElementEventHandler<Vehicle, VehicleColorChangedEventsArgs>? ColorChanged;
}
