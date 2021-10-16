using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SlipeServer.Server.Concepts
{
    public class Colors
    {
        private readonly Vehicle vehicle;

        private Color primary;
        public Color Primary
        {
            get => this.primary;
            set
            {
                var args = new VehicleColorChangedEventsArgs(this.vehicle, 0, value);
                this.primary = value;
                ColorChanged?.Invoke(this.vehicle, args);
            }
        }

        private Color secondary;
        public Color Secondary
        {
            get => this.secondary;
            set
            {
                var args = new VehicleColorChangedEventsArgs(this.vehicle, 1, value);
                this.secondary = value;
                ColorChanged?.Invoke(this.vehicle, args);
            }
        }

        private Color color3;
        public Color Color3
        {
            get => this.color3;
            set
            {
                var args = new VehicleColorChangedEventsArgs(this.vehicle, 2, value);
                this.color3 = value;
                ColorChanged?.Invoke(this.vehicle, args);
            }
        }

        private Color color4;
        public Color Color4
        {
            get => this.color4;
            set
            {
                var args = new VehicleColorChangedEventsArgs(this.vehicle, 3, value);
                this.color4 = value;
                ColorChanged?.Invoke(this.vehicle, args);
            }
        }

        public Color[] AsArray() => new Color[] { this.Primary, this.Secondary, this.Color3, this.Color4 };

        public Colors(Vehicle vehicle, Color? primary = null, Color? secondary = null, Color? color3 = null, Color? color4 = null)
        {
            this.vehicle = vehicle;
            this.primary = primary ?? Color.White;
            this.secondary = secondary ?? Color.White;
            this.color3 = color3 ?? Color.White;
            this.color4 = color4 ?? Color.White;
        }

        public event ElementEventHandler<Vehicle, VehicleColorChangedEventsArgs>? ColorChanged;
    }
}
