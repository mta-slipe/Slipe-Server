using SlipeServer.Server.Elements.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

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
        public byte TimeContext { get; private set; }

        public string Name { get; set; } = "";

        private Vector3 position;
        public Vector3 Position
        {
            get => position;
            set
            {
                PositionChanged?.Invoke(this, new ElementChangedEventArgs<Vector3>(this, value, this.IsSync));
                position = value;
            }
        }

        private Vector3 rotation;
        public Vector3 Rotation
        {
            get => rotation;
            set
            {
                RotationChanged?.Invoke(this, new ElementChangedEventArgs<Vector3>(this, value, this.IsSync));
                rotation = value;
            }
        }

        private Vector3 velocity;
        public Vector3 Velocity
        {
            get => velocity;
            set
            {
                VelocityChanged?.Invoke(this, new ElementChangedEventArgs<Vector3>(this, value, this.IsSync));
                velocity = value;
            }
        }
        
        public byte Interior { get; set; }
        public ushort Dimension { get; set; }

        public bool AreCollisionsEnabled { get; set; } = true;
        public bool IsCallPropagationEnabled { get; set; } = false;
        protected bool IsSync { get; private set; } = false;

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
            if (++TimeContext == 0)
            {
                TimeContext++;
            }
            return TimeContext;
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
        public event Action<Element>? Destroyed;
    }
}
