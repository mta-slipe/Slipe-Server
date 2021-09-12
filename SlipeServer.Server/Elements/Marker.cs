using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Server.Elements
{
    public class Marker : Element
    {
        public override ElementType ElementType => ElementType.Marker;


        private MarkerType markerType;
        public MarkerType MarkerType
        {
            get => this.markerType;
            set
            {
                var args = new ElementChangedEventArgs<Marker, MarkerType>(this, this.markerType, value, this.IsSync);
                this.markerType = value;
                MarkerTypeChanged?.Invoke(this, args);
            }
        }
        
        private MarkerIcon markerIcon;
        public MarkerIcon MarkerIcon
        {
            get => this.markerIcon;
            set
            {
                var args = new ElementChangedEventArgs<Marker, MarkerIcon>(this, this.markerIcon, value, this.IsSync);
                this.markerIcon = value;
                MarkerIconChanged?.Invoke(this, args);
            }
        }

        private float size = 1;
        public float Size
        {
            get => this.size;
            set
            {
                var args = new ElementChangedEventArgs<Marker, float>(this, this.size, value, this.IsSync);
                this.size = value;
                SizeChanged?.Invoke(this, args);
            }
        }

        private Color color;
        public Color Color
        {
            get => this.color;
            set
            {
                var args = new ElementChangedEventArgs<Marker, Color>(this, this.color, value, this.IsSync);
                this.color = value;
                ColorChanged?.Invoke(this, args);
            }
        }

        private Vector3? targetPosition;
        public Vector3? TargetPosition
        {
            get => this.targetPosition;
            set
            {
                var args = new ElementChangedEventArgs<Marker, Vector3?>(this, this.targetPosition, value, this.IsSync);
                this.targetPosition = value;
                TargetPositionChanged?.Invoke(this, args);
            }
        }

        public Marker(Vector3 position, MarkerType markerType)
        {
            this.Position = position;
            this.MarkerType = markerType;
        }

        public new Marker AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public event ElementChangedEventHandler<Marker, MarkerType>? MarkerTypeChanged;
        public event ElementChangedEventHandler<Marker, MarkerIcon>? MarkerIconChanged;
        public event ElementChangedEventHandler<Marker, float>? SizeChanged;
        public event ElementChangedEventHandler<Marker, Color>? ColorChanged;
        public event ElementChangedEventHandler<Marker, Vector3?>? TargetPositionChanged;
    }
}
