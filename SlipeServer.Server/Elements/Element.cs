using SlipeServer.Server.Elements.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements
{
    public class Element
    {
        public virtual ElementType ElementType => ElementType.Unknown;

        private Element? parent;
        public Element? Parent
        {
            get => parent;
            set
            {
                this.parent = value;

                if (this.parent != null)
                    this.parent.RemoveChild(this);

                if (value != null)
                    value.AddChild(this);
            }
        }

        private readonly List<Element> children;
        public IReadOnlyCollection<Element> Children => children.AsReadOnly();

        public uint Id { get; set; }

        private readonly object timeContextLock = new object();
        public byte TimeContext { get; private set; }

        public string Name { get; set; } = "";

        protected Vector3 position;
        public Vector3 Position
        {
            get => position;
            set
            {
                var args = new ElementChangedEventArgs<Vector3>(this, this.Position, value, this.IsSync);
                position = value;
                PositionChanged?.Invoke(this, args);
            }
        }

        protected Vector3 rotation;
        public Vector3 Rotation
        {
            get => rotation;
            set
            {
                var args = new ElementChangedEventArgs<Vector3>(this, this.Rotation, value, this.IsSync);
                rotation = value;
                RotationChanged?.Invoke(this, args);
            }
        }

        protected Vector3 velocity;
        public Vector3 Velocity
        {
            get => velocity;
            set
            {
                var args = new ElementChangedEventArgs<Vector3>(this, this.Velocity, value, this.IsSync);
                velocity = value;
                VelocityChanged?.Invoke(this, args);
            }
        }

        protected byte interior;
        public byte Interior
        {
            get => interior;
            set
            {
                var args = new ElementChangedEventArgs<byte>(this, this.Interior, value, this.IsSync);
                interior = value;
                InteriorChanged?.Invoke(this, args);
            }
        }

        protected ushort dimension;
        public ushort Dimension
        {
            get => dimension;
            set
            {
                var args = new ElementChangedEventArgs<ushort>(this, this.Dimension, value, this.IsSync);
                dimension = value;
                DimensionChanged?.Invoke(this, args);
            }
        }

        protected byte alpha = 255;
        public byte Alpha
        {
            get => alpha;
            set
            {
                var args = new ElementChangedEventArgs<byte>(this, this.Alpha, value, this.IsSync);
                alpha = value;
                AlphaChanged?.Invoke(this, args);
            }
        }

        public bool AreCollisionsEnabled { get; set; } = true;
        public bool IsCallPropagationEnabled { get; set; } = false;


        private AsyncLocal<bool> isSync = new AsyncLocal<bool>();
        public bool IsSync
        {
            get => this.isSync?.Value ?? false;
            protected set
            {
                this.isSync ??= new AsyncLocal<bool>();
                this.isSync.Value = value;
            }
        }


        public Element()
        {
            this.children = new List<Element>();
        }

        public Element(Element parent) : this()
        {
            Parent = parent;
        }

        public byte GetAndIncrementTimeContext()
        {
            lock (timeContextLock)
            {
                if (++TimeContext == 0)
                {
                    TimeContext++;
                }
                return TimeContext;
            }
        }

        public void Destroy()
        {
            this.Destroyed?.Invoke(this);
        }

        public void RunAsSync(Action action)
        {
            this.IsSync = true;
            action();
            this.IsSync = false;
        }

        public async Task RunAsSync(Func<Task> action)
        {
            this.IsSync = true;
            await action();
            this.IsSync = false;
        }

        public Element AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public void AddChild(Element element)
        {
            this.children.Add(element);
            element.Destroyed += (element) => RemoveChild(element);
        }

        public void RemoveChild(Element element)
        {
            this.children.Remove(element);
        }

        public event ElementChangedEventHandler<Vector3>? PositionChanged;
        public event ElementChangedEventHandler<Vector3>? RotationChanged;
        public event ElementChangedEventHandler<Vector3>? VelocityChanged;
        public event ElementChangedEventHandler<byte>? InteriorChanged;
        public event ElementChangedEventHandler<ushort>? DimensionChanged;
        public event ElementChangedEventHandler<byte>? AlphaChanged;
        public event Action<Element>? Destroyed;
    }
}
