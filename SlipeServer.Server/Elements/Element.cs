using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements
{
    public static class Matrix4x4Extensions
    {
        public static Vector3 Transform(this Matrix4x4 matrix, Vector3 offset)
        {
            matrix.M14 = 0;
            matrix.M24 = 0;
            matrix.M34 = 0;
            matrix.M44 = 1;
            Matrix4x4.Invert(matrix, out matrix);
            return -(new Vector3
            {
                X = offset.X * matrix.M11 + offset.Y * matrix.M21 + offset.Z * matrix.M31 + matrix.M41,
                Y = offset.X * matrix.M12 + offset.Y * matrix.M22 + offset.Z * matrix.M32 + matrix.M42,
                Z = offset.X * matrix.M13 + offset.Y * matrix.M23 + offset.Z * matrix.M33 + matrix.M43
            });
        }
    }
    public class Element
    {
        public virtual ElementType ElementType => ElementType.Unknown;

        private Element? parent;
        public Element? Parent
        {
            get => this.parent;
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
        public IReadOnlyCollection<Element> Children => this.children.AsReadOnly();

        public uint Id { get; set; }

        private readonly object timeContextLock = new();
        public byte TimeContext { get; private set; }

        private string name = "";
        public string Name
        {
            get => this.name;
            set
            {
                var args = new ElementChangedEventArgs<string>(this, this.Name, value, this.IsSync);
                this.name = value;
                NameChanged?.Invoke(this, args);
            }
        }

        public Matrix4x4 AttachOffsetMatrix
        {
            get
            {
                if (this.AttachedToElement != null)
                    return Matrix4x4.CreateTranslation(this.attachedPositionOffset.X, this.attachedPositionOffset.Y, this.attachedPositionOffset.Z) * Matrix4x4.CreateFromYawPitchRoll(this.attachedRotationOffset.X, this.attachedRotationOffset.Y, -this.attachedRotationOffset.Z + MathF.PI);

                return Matrix4x4.Identity;
            }
        }

        public Matrix4x4 Matrix {
            get
            {
                if (this.attachedToElement != null)
                {
                    return this.attachedToElement.Matrix * Matrix4x4.CreateTranslation(-(this.attachedToElement.attachedPositionOffset + this.attachedPositionOffset));
                }
                return Matrix4x4.CreateTranslation(this.position.X, this.position.Y, this.position.Z) * Matrix4x4.CreateFromYawPitchRoll(this.rotation.X, this.rotation.Y, -this.rotation.Z + MathF.PI);
            }
        }

        protected Vector3 position;
        public Vector3 Position
        {
            get
            {
                if (this.AttachedToElement != null)
                    return (this.AttachedToElement.Matrix).Transform(this.AttachedPositionOffset);
                return this.position;
            }
            set
            {
                var args = new ElementChangedEventArgs<Vector3>(this, this.Position, value, this.IsSync);
                this.position = value;
                PositionChanged?.Invoke(this, args);
            }
        }

        protected Vector3 rotation;
        public Vector3 Rotation
        {
            get
            {
                if (this.AttachedToElement != null)
                {
                    return this.AttachedRotationOffset + this.AttachedToElement.Rotation;
                }
                return this.rotation;
            }
            set
            {
                var args = new ElementChangedEventArgs<Vector3>(this, this.Rotation, value, this.IsSync);
                this.rotation = value;
                RotationChanged?.Invoke(this, args);
            }
        }

        protected Vector3 velocity;
        public Vector3 Velocity
        {
            get => this.velocity;
            set
            {
                var args = new ElementChangedEventArgs<Vector3>(this, this.Velocity, value, this.IsSync);
                this.velocity = value;
                VelocityChanged?.Invoke(this, args);
            }
        }

        protected Vector3 turnVelocity;
        public Vector3 TurnVelocity
        {
            get => this.turnVelocity;
            set
            {
                var args = new ElementChangedEventArgs<Vector3>(this, this.TurnVelocity, value, this.IsSync);
                this.turnVelocity = value;
                TurnVelocityChanged?.Invoke(this, args);
            }
        }

        protected byte interior;
        public byte Interior
        {
            get => this.interior;
            set
            {
                var args = new ElementChangedEventArgs<byte>(this, this.Interior, value, this.IsSync);
                this.interior = value;
                InteriorChanged?.Invoke(this, args);
            }
        }

        protected ushort dimension;
        public ushort Dimension
        {
            get => this.dimension;
            set
            {
                var args = new ElementChangedEventArgs<ushort>(this, this.Dimension, value, this.IsSync);
                this.dimension = value;
                DimensionChanged?.Invoke(this, args);
            }
        }

        protected byte alpha = 255;
        public byte Alpha
        {
            get => this.alpha;
            set
            {
                var args = new ElementChangedEventArgs<byte>(this, this.Alpha, value, this.IsSync);
                this.alpha = value;
                AlphaChanged?.Invoke(this, args);
            }
        }


        protected bool areCollisionsEnabled = true;
        public bool AreCollisionsEnabled
        {
            get => this.areCollisionsEnabled;
            set
            {
                var args = new ElementChangedEventArgs<bool>(this, this.areCollisionsEnabled, value, this.IsSync);
                this.areCollisionsEnabled = value;
                CollisionEnabledhanged?.Invoke(this, args);
            }
        }

        protected bool isCallPropagationEnabled = false;
        public bool IsCallPropagationEnabled
        {
            get => this.isCallPropagationEnabled;
            set
            {
                var args = new ElementChangedEventArgs<bool>(this, this.isCallPropagationEnabled, value, this.IsSync);
                this.isCallPropagationEnabled = value;
                CallPropagationChanged?.Invoke(this, args);
            }
        }

        private AsyncLocal<bool> isSync = new();
        public bool IsSync
        {
            get => this.isSync?.Value ?? false;
            protected set
            {
                this.isSync ??= new AsyncLocal<bool>();
                this.isSync.Value = value;
            }
        }

        private readonly HashSet<Player> subscribers;
        public IEnumerable<Player> Subscribers => this.subscribers;


        public Element()
        {
            this.children = new();
            this.subscribers = new();
            this.TimeContext = 1;
        }

        public Element(Element parent) : this()
        {
            this.Parent = parent;
        }

        public void AddSubscriber(Player player)
        {
            if (this.subscribers.Contains(player))
                return;

            this.subscribers.Add(player);
            player.SubscribeTo(this);
        }

        public void RemoveSubscriber(Player player)
        {
            if (!this.subscribers.Contains(player))
                return;

            this.subscribers.Remove(player);
            player.UnsubscribeFrom(this);
        }


        public byte GetAndIncrementTimeContext()
        {
            lock (this.timeContextLock)
            {
                if (++this.TimeContext == 0)
                {
                    this.TimeContext++;
                }
                return this.TimeContext;
            }
        }

        public void Destroy()
        {
            this.Destroyed?.Invoke(this);
        }

        public void RunAsSync(Action action, bool value = true)
        {
            var oldValue = this.IsSync;
            this.IsSync = value;
            action();
            this.IsSync = oldValue;
        }

        public async Task RunAsSync(Func<Task> action, bool value = true)
        {
            var oldValue = this.IsSync;
            this.IsSync = value;
            await action();
            this.IsSync = oldValue;
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

        public bool IsChildOf(Element element)
        {
            return element != null && (this.parent == element || (this.parent != null && this.parent.IsChildOf(element)));
        }

        public void CreateFor(IEnumerable<Player> players)
            => AddEntityPacketFactory.CreateAddEntityPacket(new Element[] { this }).SendTo(players);

        public void CreateFor(Player player)
            => this.CreateFor(new Player[] { player });

        public void DestroyFor(IEnumerable<Player> players)
            => RemoveEntityPacketFactory.CreateRemoveEntityPacket(new Element[] { this }).SendTo(players);

        public void DestroyFor(Player player)
            => this.DestroyFor(new Player[] { player });

        private Vector3 attachedPositionOffset = Vector3.Zero;
        private Vector3 attachedRotationOffset = Vector3.Zero;
        private Element? attachedToElement = null;
        public Vector3 AttachedPositionOffset { get => this.attachedPositionOffset; set
            {
                this.attachedPositionOffset = value;
                this.AttachedOffsetChanged?.Invoke(this, new ElementAttachOffsetsChangedArgs(this, this.attachedPositionOffset, this.attachedRotationOffset));
            }
        }
        public Vector3 AttachedRotationOffset
        {
            get => this.attachedRotationOffset; set
            {
                this.attachedRotationOffset = value;
                this.AttachedOffsetChanged?.Invoke(this, new ElementAttachOffsetsChangedArgs(this, this.attachedPositionOffset, this.attachedRotationOffset));
            }
        }
        public Element? AttachedToElement { get => this.attachedToElement; private set => this.attachedToElement = value; }
        private readonly HashSet<Element> attachedElements = new();
        public IReadOnlyCollection<Element> AttachedElements => this.attachedElements;
        public void AttachElement(Element other, Vector3? positionOffset = null, Vector3? rotationOffset = null)
        {
            if (this.attachedElements.Add(other))
            {
                other.AttachedToElement = this;
                other.attachedPositionOffset = positionOffset ?? Vector3.Zero;
                other.attachedRotationOffset = rotationOffset ?? Vector3.Zero;
                Attached?.Invoke(this, new ElementAttachedEventArgs(other, this, positionOffset ?? Vector3.Zero, rotationOffset ?? Vector3.Zero));
            }
        }

        public event ElementChangedEventHandler<Vector3>? PositionChanged;
        public event ElementChangedEventHandler<Vector3>? RotationChanged;
        public event ElementChangedEventHandler<Vector3>? VelocityChanged;
        public event ElementChangedEventHandler<Vector3>? TurnVelocityChanged;
        public event ElementChangedEventHandler<byte>? InteriorChanged;
        public event ElementChangedEventHandler<ushort>? DimensionChanged;
        public event ElementChangedEventHandler<byte>? AlphaChanged;
        public event ElementChangedEventHandler<string>? NameChanged;
        public event ElementChangedEventHandler<bool>? CallPropagationChanged;
        public event ElementChangedEventHandler<bool>? CollisionEnabledhanged;
        public event ElementEventHandler<Element, ElementAttachedEventArgs>? Attached;
        public event ElementEventHandler<Element, ElementAttachOffsetsChangedArgs>? AttachedOffsetChanged;
        public event Action<Element>? Destroyed;
    }
}
