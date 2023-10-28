using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Extensions.Relaying;
using SlipeServer.Server.PacketHandling.Factories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SlipeServer.Server.Elements;

/// <summary>
/// The base class for any and all elements
/// </summary>
public class Element
{
    /// <summary>
    /// The element type as per MTA's specifications. Multiple C# types can share the same MTA element type.
    /// </summary>
    public virtual ElementType ElementType => ElementType.Unknown;

    private Element? parent;
    /// <summary>
    /// The element's parent as in the element tree.
    /// </summary>
    public Element? Parent
    {
        get => this.parent;
        set
        {
            this.parent = value;

            this.parent?.RemoveChild(this);
            value?.AddChild(this);
        }
    }

    private readonly object childrenLock = new();
    private readonly List<Element> children;
    /// <summary>
    /// The element's children as in the element tree.
    /// </summary>
    public IReadOnlyCollection<Element> Children => this.children.AsReadOnly();

    private readonly List<ElementAssociation> associations;
    public IReadOnlyCollection<ElementAssociation> Associations => this.associations.AsReadOnly();

    private readonly HashSet<Player> associatedPlayers;
    /// <summary>
    /// Players the element is associated with. These are players that are aware of the existence of this element.
    /// </summary>
    public IEnumerable<Player> AssociatedPlayers => this.associatedPlayers.AsEnumerable();

    private ElementId id;
    public ElementId Id
    {
        get => this.id;
        set
        {
            if (this.id == value)
                return;

            var args = new ElementChangedEventArgs<ElementId>(this, this.id, value, this.IsSync);
            this.id = value;
            IdChanged?.Invoke(this, args);
        }
    }

    private readonly object timeContextLock = new();
    /// <summary>
    /// The time sync context, this is a value used to verify whether synchronisation packets are to be applied or ignored.
    /// </summary>
    public byte TimeContext { get; private set; }


    private string name = "";
    /// <summary>
    /// The element name, for Player's this is the nickname, for other elements it's mostly unused.
    /// </summary>
    public string Name
    {
        get => this.name;
        set
        {
            if (this.name == value)
                return;

            var args = new ElementChangedEventArgs<string>(this, this.Name, value, this.IsSync);
            this.name = value;
            NameChanged?.Invoke(this, args);
        }
    }


    protected Vector3 position;
    /// <summary>
    /// The element's position
    /// </summary>
    public Vector3 Position
    {
        get => this.position;
        set
        {
            if (this.position == value)
                return;

            var args = new ElementChangedEventArgs<Vector3>(this, this.Position, value, this.IsSync);
            this.position = value;
            PositionChanged?.Invoke(this, args);

            foreach (var attachment in this.attachedElements)
                attachment.UpdateAttachedElement();
        }
    }

    /// <summary>
    /// A vector representating the location 1 unit to the right of the element.
    /// </summary>
    public Vector3 Right => Vector3.Transform(Vector3.UnitX, this.rotation.ToQuaternion());

    /// <summary>
    /// A vector representing the location 1 unit above the element.
    /// </summary>
    public Vector3 Up => Vector3.Transform(Vector3.UnitZ, this.rotation.ToQuaternion());

    /// <summary>
    /// A vector representing 1 unit in front of the element.
    /// </summary>
    public Vector3 Forward => Vector3.Transform(Vector3.UnitY, this.rotation.ToQuaternion());

    protected Vector3 rotation;
    /// <summary>
    /// The element's rotation, in euler angles
    /// </summary>
    public Vector3 Rotation
    {
        get => this.rotation;
        set
        {
            if (this.rotation == value)
                return;

            var args = new ElementChangedEventArgs<Vector3>(this, this.Rotation, value, this.IsSync);
            this.rotation = value;
            RotationChanged?.Invoke(this, args);

            foreach (var attachment in this.attachedElements)
                attachment.UpdateAttachedElement();
        }
    }

    protected Vector3 velocity;
    /// <summary>
    /// The element's current velocity.
    /// </summary>
    public Vector3 Velocity
    {
        get => this.velocity;
        set
        {
            if (this.velocity == value)
                return;

            var args = new ElementChangedEventArgs<Vector3>(this, this.Velocity, value, this.IsSync);
            this.velocity = value;
            VelocityChanged?.Invoke(this, args);
        }
    }

    protected Vector3 turnVelocity;
    /// <summary>
    /// The element's turn velocity.
    /// </summary>
    public Vector3 TurnVelocity
    {
        get => this.turnVelocity;
        set
        {
            if (this.turnVelocity == value)
                return;

            var args = new ElementChangedEventArgs<Vector3>(this, this.TurnVelocity, value, this.IsSync);
            this.turnVelocity = value;
            TurnVelocityChanged?.Invoke(this, args);
        }
    }

    protected byte interior;
    /// <summary>
    /// The element's interior, interiors are the mechanism the base game uses to not have the insides of buildings visible in the regular world.
    /// </summary>
    public byte Interior
    {
        get => this.interior;
        set
        {
            if (this.interior == value)
                return;

            var args = new ElementChangedEventArgs<byte>(this, this.Interior, value, this.IsSync);
            this.interior = value;
            InteriorChanged?.Invoke(this, args);
        }
    }

    protected ushort dimension;
    /// <summary>
    /// The element's dimension, elements are only visible in the dimension a player (or rather their camera) is in.
    /// </summary>
    public ushort Dimension
    {
        get => this.dimension;
        set
        {
            if (this.dimension == value)
                return;

            var args = new ElementChangedEventArgs<ushort>(this, this.Dimension, value, this.IsSync);
            this.dimension = value;
            DimensionChanged?.Invoke(this, args);
        }
    }

    protected byte alpha = 255;
    /// <summary>
    /// The element's alpha level, alpha of 0 means completely transparent, while 255 is completely opaque.
    /// </summary>
    public byte Alpha
    {
        get => this.alpha;
        set
        {
            if (this.alpha == value)
                return;

            var args = new ElementChangedEventArgs<byte>(this, this.Alpha, value, this.IsSync);
            this.alpha = value;
            AlphaChanged?.Invoke(this, args);
        }
    }


    protected bool areCollisionsEnabled = true;
    /// <summary>
    /// Indicates whether the element collides with anything else, this includes both world objects, and the default map.
    /// </summary>
    public bool AreCollisionsEnabled
    {
        get => this.areCollisionsEnabled;
        set
        {
            if (this.areCollisionsEnabled == value)
                return;

            var args = new ElementChangedEventArgs<bool>(this, this.areCollisionsEnabled, value, this.IsSync);
            this.areCollisionsEnabled = value;
            CollisionEnabledChanged?.Invoke(this, args);
        }
    }

    protected bool isCallPropagationEnabled = false;
    /// <summary>
    /// Indicates whether a call on this element will be called on its children a well on clients.
    /// </summary>
    /// <remarks>
    /// Note: This only affects clients, Slipe Server does not do call propagation.
    /// </remarks>
    public bool IsCallPropagationEnabled
    {
        get => this.isCallPropagationEnabled;
        set
        {
            if (this.isCallPropagationEnabled == value)
                return;

            var args = new ElementChangedEventArgs<bool>(this, this.isCallPropagationEnabled, value, this.IsSync);
            this.isCallPropagationEnabled = value;
            CallPropagationChanged?.Invoke(this, args);
        }
    }

    protected bool isFrozen = false;
    /// <summary>
    /// Indicates whether the element is frozen, frozen elements are unable to move or be moved.
    /// </summary>
    public bool IsFrozen
    {
        get => this.isFrozen;
        set
        {
            if (this.isFrozen == value)
                return;

            var args = new ElementChangedEventArgs<bool>(this, this.isFrozen, value, this.IsSync);
            this.isFrozen = value;
            FrozenChanged?.Invoke(this, args);
        }
    }

    private AsyncLocal<ElementUpdateContext> updateContext = new();
    /// <summary>
    /// The element's update context, this indicates the source of the current change that is being applied to an element.
    /// This is primarily used to verify whether or not the current changes originate from a sync packet.
    /// </summary>
    public ElementUpdateContext UpdateContext
    {
        get => this.updateContext?.Value ?? ElementUpdateContext.Default;
        protected set
        {
            this.updateContext ??= new AsyncLocal<ElementUpdateContext>();
            this.updateContext.Value = value;
        }
    }

    /// <summary>
    /// Indicates whether the current action originates from the handling of a sync packet
    /// </summary>
    public bool IsSync
    {
        get => this.UpdateContext.HasFlag(ElementUpdateContext.Sync);
        set => this.UpdateContext = ElementUpdateContext.Sync;
    }

    private readonly HashSet<Player> subscribers;
    /// <summary>
    /// Indicates which elements are subscribed to this element
    /// </summary>
    public IEnumerable<Player> Subscribers => this.subscribers;

    private Dictionary<string, ElementData> ElementData { get; set; }
    /// <summary>
    /// Lists all the players that are subscribed to one (or more) element data entries on this element.
    /// </summary>
    public ConcurrentDictionary<Player, ConcurrentDictionary<string, bool>> ElementDataSubscriptions { get; set; }

    /// <summary>
    /// A read-only representation of all element data that is broadcastable
    /// </summary>
    public Packets.Definitions.Entities.Structs.CustomData BroadcastableElementData => new()
    {
        Items = this.ElementData
            .Where(x => x.Value.SyncType == DataSyncType.Broadcast)
            .Select(x => new Packets.Definitions.Entities.Structs.CustomDataItem()
            {
                Name = x.Key,
                Data = x.Value.Value
            })
    };

    /// <summary>
    /// The element's current attachment. Representing this element being attached to a target element.
    /// </summary>
    public ElementAttachment? Attachment { get; private set; }

    private readonly List<ElementAttachment> attachedElements;
    /// <summary>
    /// Elements that are attached to this element
    /// </summary>
    public IReadOnlyCollection<ElementAttachment> AttachedElements => this.attachedElements.AsReadOnly();

    /// <summary>
    /// Indicates whether this element has been destroyed
    /// </summary>
    public bool IsDestroyed { get; set; }

    private readonly object destroyLock = new();

    public Element()
    {
        this.children = new();
        this.associations = new();
        this.associatedPlayers = new();
        this.subscribers = new();
        this.attachedElements = new();
        this.TimeContext = 1;

        this.ElementData = new();
        this.ElementDataSubscriptions = new();

        this.AddRelayers();
    }

    public Element(Element parent) : this()
    {
        this.Parent = parent;
    }

    /// <summary>
    /// Adds a subscriber to this element
    /// </summary>
    /// <param name="player">the subscriber to add to this element</param>
    public void AddSubscriber(Player player)
    {
        if (this.subscribers.Contains(player))
            return;

        this.subscribers.Add(player);
        player.SubscribeTo(this);
    }

    /// <summary>
    /// Removes a subscriber from this element
    /// </summary>
    /// <param name="player">the subscriber to remove from this element</param>
    public void RemoveSubscriber(Player player)
    {
        if (!this.subscribers.Contains(player))
            return;

        this.subscribers.Remove(player);
        player.UnsubscribeFrom(this);
    }

    /// <summary>
    /// Returns a new time context, to be used when sync updates sent prior to this moment are meant to be invaldiated.
    /// </summary>
    /// <returns>The new time context</returns>
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

    /// <summary>
    /// Indicates whether a specific time context is valid to update this element
    /// </summary>
    /// <param name="remoteContext">The time context to compare</param>
    public bool CanUpdateSync(byte remoteContext)
    {
        return (this.TimeContext == remoteContext || remoteContext == 0 || this.TimeContext == 0);
    }

    /// <summary>
    /// Destroys the element, triggering the Destroyed event.
    /// </summary>
    /// <returns>A bool indicating whether the element is destroyed, false if it was already destroyed prior to this call.</returns>
    public bool Destroy()
    {
        lock (this.destroyLock)
        {
            if (this.IsDestroyed)
                return false;

            this.IsDestroyed = true;
            this.Destroyed?.Invoke(this);
            return true;
        }
    }

    /// <summary>
    /// Runs an action with a specific context, this context can be fetched using `element.UpdateContext`
    /// </summary>
    /// <param name="action">the action to run</param>
    /// <param name="context">the context to run with</param>
    public void RunWithContext(Action action, ElementUpdateContext context)
    {
        var oldValue = this.UpdateContext;
        this.UpdateContext = context;
        action();
        this.UpdateContext = oldValue;
    }

    /// <summary>
    /// Runs an asynchronous action with a specific context, this context can be fetched using `element.UpdateContext`
    /// </summary>
    /// <param name="action">the action to run</param>
    /// <param name="context">the context to run with</param>
    /// <returns></returns>
    public async Task RunWithContext(Func<Task> action, ElementUpdateContext context)
    {
        var oldValue = this.UpdateContext;
        this.UpdateContext = context;
        await action();
        this.UpdateContext = oldValue;
    }

    /// <summary>
    /// Runs an action with the sync context
    /// </summary>
    /// <param name="action">the action to run</param>
    /// <param name="value">Depending whether the context should be sync or not</param>
    public void RunAsSync(Action action, bool value = true)
        => RunWithContext(action, value ? ElementUpdateContext.Sync : ElementUpdateContext.NoRelay);

    /// <summary>
    /// Runs an asynchronous action with the sync context
    /// </summary>
    /// <param name="action">the action to run</param>
    /// <param name="value">Depending whether the context should be sync or not</param>
    public Task RunAsSync(Func<Task> action, bool value = true)
        => RunWithContext(action, value ? ElementUpdateContext.Sync : ElementUpdateContext.NoRelay);

    /// <summary>
    /// Associates an element with the server, causing the element to be created for all players on the server
    /// </summary>
    /// <param name="server">The server to associate the element with</param>
    public Element AssociateWith(MtaServer server)
    {
        this.associations.Add(new ElementAssociation(this, server));
        server.AssociateElement(this);

        this.AssociatedWith?.Invoke(this, new ElementAssociatedWithEventArgs(this, server));

        this.UpdateAssociatedPlayers();

        server.PlayerJoined += HandlePlayerJoin;
        server.ElementCreated += HandleElementCreation;

        return this;
    }


    /// <summary>
    /// Removes an element from being associated with the server, causing the elements to no longer be created for all players
    /// </summary>
    /// <param name="server"></param>
    public void RemoveFrom(MtaServer server)
    {
        var associations = this.associations.Where(x => x.Element == this && x.Server == server);

        foreach (var association in associations)
            this.associations.Remove(association);

        server.PlayerJoined -= HandlePlayerJoin;

        server.RemoveElement(this);
        this.RemovedFrom?.Invoke(this, new ElementAssociatedWithEventArgs(this, server));
        this.UpdateAssociatedPlayers();
    }

    private void HandlePlayerJoin(Player player)
    {
        this.associatedPlayers.Add(player);

        player.Disconnected += HandlePlayerDisconnect;
    }

    private void HandlePlayerDisconnect(Player player, PlayerQuitEventArgs args)
    {
        this.associatedPlayers.Remove(player);
    }

    private void HandleElementCreation(Element obj)
    {
        obj.UpdateAssociatedPlayers();
    }

    /// <summary>
    /// Associates an element with a player, causing the element to be created for this player
    /// </summary>
    /// <param name="server">The server to associate the element with</param>
    public Element AssociateWith(Player player)
    {
        player.AssociateElement(this);

        this.associations.Add(new ElementAssociation(this, player));
        this.UpdateAssociatedPlayers();
        this.AssociatedWith?.Invoke(this, new ElementAssociatedWithEventArgs(this, player));

        return this;
    }

    /// <summary>
    /// Removes an element from being associated with the server, causing the elements to no longer be created for all players
    /// </summary>
    /// <param name="server"></param>
    public void RemoveFrom(Player player)
    {
        var associations = this.associations
            .Where(x => x.Element == this && x.Player == player)
            .ToArray();

        foreach (var association in associations)
            this.associations.Remove(association);

        player.RemoveElement(this);
        this.RemovedFrom?.Invoke(this, new ElementAssociatedWithEventArgs(this, player));
        this.UpdateAssociatedPlayers();
    }

    protected void UpdateAssociatedPlayers()
    {
        this.associatedPlayers.Clear();

        if (this is Player thisPlayer)
            this.associatedPlayers.Add(thisPlayer);

        foreach (var association in this.associations)
        {
            if (association.Player != null)
                this.associatedPlayers.Add(association.Player);
            else if (association.Server != null)
                foreach (var player in association.Server.Players)
                    this.associatedPlayers.Add(player);
        }
    }

    /// <summary>
    /// Adds a child in the element tree to this element
    /// </summary>
    /// <param name="element">The child to add</param>
    public void AddChild(Element element)
    {
        lock (this.childrenLock)
        {
            this.children.Add(element);
            element.Destroyed += RemoveChild;
        }
    }

    /// <summary>
    /// Removes a child from this tree in the element tree
    /// </summary>
    /// <param name="element">The child to remove</param>
    public void RemoveChild(Element element)
    {
        lock (this.childrenLock)
        {
            this.children.Remove(element);
            element.Destroyed -= RemoveChild;
        }
    }

    /// <summary>
    /// Indicates whether the element is an (indirect) child of another element, this includes grandchildren.
    /// </summary>
    /// <param name="element">The element to check against</param>
    public bool IsChildOf(Element element)
    {
        return element != null && (this.parent == element || (this.parent != null && this.parent.IsChildOf(element)));
    }

    /// <summary>
    /// Sets element data on this element
    /// </summary>
    /// <param name="key">The key to store the value under</param>
    /// <param name="value">The value to store</param>
    /// <param name="syncType">The type of synchronisation to do with this value</param>
    public void SetData(string key, LuaValue value, DataSyncType syncType = DataSyncType.Local)
    {
        LuaValue? oldValue = this.GetData(key);

        if (value.IsNil)
            this.ElementData.Remove(key);
        else
            this.ElementData[key] = new ElementData(key, value, syncType);

        this.DataChanged?.Invoke(this, new ElementDataChangedArgs(key, value, oldValue, syncType));
    }

    /// <summary>
    /// Gets element data on this element if it exists
    /// </summary>
    /// <param name="dataName">The key to retrieve the stored value from</param>
    /// <param name="inherit">Whether the value should be attempted to be fetched from the element's parent(s) as well</param>
    /// <returns>The value if it exists, null otherwise.</returns>
    public LuaValue? GetData(string dataName, bool inherit = false)
    {
        if (this.ElementData.TryGetValue(dataName, out var value))
            return value.Value;

        if (inherit)
            return this.parent?.GetData(dataName, inherit);

        return null;
    }

    /// <summary>
    /// Subscribes a player to changes to a specific element data value
    /// </summary>
    /// <param name="player">The player to subscribe</param>
    /// <param name="key">The key of the data to subscribe to</param>
    public void SubscribeToData(Player player, string key)
    {
        if (!this.ElementDataSubscriptions.ContainsKey(player))
        {
            this.ElementDataSubscriptions[player] = new();
            player.Destroyed += HandleElementDataSubscriberDestruction;
        }
        this.ElementDataSubscriptions[player].TryAdd(key, true);

    }

    private void HandleElementDataSubscriberDestruction(Element element)
    {
        if (element is Player player)
            UnsubscribeFromAllData(player);
    }

    /// <summary>
    /// Unsubscribes a player from changes to a specific element data value
    /// </summary>
    /// <param name="player">The player to unsubscribe</param>
    /// <param name="key">The key of the data to unsubscribe from</param>
    public void UnsubscribeFromData(Player player, string key)
    {
        if (this.ElementDataSubscriptions.TryGetValue(player, out var keys))
        {
            keys.Remove(key, out var _);
            if (keys.IsEmpty)
                UnsubscribeFromAllData(player);
        }
    }

    /// <summary>
    /// Unsubscribes an element from all element data on this element
    /// </summary>
    /// <param name="player"></param>
    public void UnsubscribeFromAllData(Player player)
    {
        player.Destroyed -= HandleElementDataSubscriberDestruction;
        this.ElementDataSubscriptions.Remove(player, out var keys);
    }

    /// <summary>
    /// Checks whether a player is subscribes to a specific data key on this element
    /// </summary>
    /// <param name="player">The player to check</param>
    /// <param name="key">The key for the element data to check</param>
    /// <returns></returns>
    public bool IsPlayerSubscribedToData(Player player, string key)
    {
        if (this.ElementDataSubscriptions.TryGetValue(player, out var keys))
            return keys.ContainsKey(key);

        return false;
    }

    /// <summary>
    /// Returns all players subscribes to a specific element data key on this element
    /// </summary>
    /// <param name="key">The element data key</param>
    public IEnumerable<Player> GetPlayersSubcribedToData(string key)
    {
        return this.ElementDataSubscriptions.Keys
            .Where(x => this.ElementDataSubscriptions[x].ContainsKey(key));
    }

    internal void AddElementAttachment(ElementAttachment attachment) => this.attachedElements.Add(attachment);
    internal void RemoveElementAttachment(ElementAttachment attachment) => this.attachedElements.Remove(attachment);

    /// <summary>
    /// Attaches the element to a target element
    /// </summary>
    /// <param name="element">Target element to attach to</param>
    /// <param name="positionOffset">position offset between the element, and the target element</param>
    /// <param name="rotationOffset">rotation offset between the element, and the target element</param>
    /// <returns></returns>
    /// <exception cref="Exception">Throws an exception when the target element does not have an ID</exception>
    public ElementAttachment AttachTo(Element element, Vector3? positionOffset = null, Vector3? rotationOffset = null)
    {
        if (element.Id.Value == 0)
            throw new Exception($"Can not attach {this} to {element} because {element} has no id");

        var position = positionOffset ?? Vector3.Zero;
        var rotation = rotationOffset ?? Vector3.Zero;

        if (this.Attachment != null)
            DetachFrom(this.Attachment.Target);

        var attachment = new ElementAttachment(this, element, position, rotation);
        this.Attachment = attachment;
        element.AddElementAttachment(attachment);
        attachment.UpdateAttachedElement();

        attachment.PositionOffsetChanged += HandleAttachmentPositionOffsetChange;
        attachment.RotationOffsetChanged += HandleAttachmentRotationOffsetChange;

        this.Attached?.Invoke(this, new ElementAttachedEventArgs(this, element, position, rotation));

        return attachment;
    }

    private void HandleAttachmentPositionOffsetChange(Vector3 newPosition)
    {
        this.AttachedOffsetChanged?.Invoke(this, new ElementAttachOffsetsChangedArgs(this, this.Attachment!.Target, newPosition, this.Attachment.RotationOffset));
    }

    private void HandleAttachmentRotationOffsetChange(Vector3 newRotation)
    {
        this.AttachedOffsetChanged?.Invoke(this, new ElementAttachOffsetsChangedArgs(this, this.Attachment!.Target, this.Attachment.PositionOffset, newRotation));
    }

    /// <summary>
    /// Detaches the element from a target element
    /// </summary>
    /// <param name="element">Target element to detach from</param>
    public virtual void DetachFrom(Element? element = null)
    {
        if (this.Attachment != null && (element == null || this.Attachment.Target == element))
        {
            this.Detached?.Invoke(this, new ElementDetachedEventArgs(this, this.Attachment.Target));
            (element ?? this.Attachment.Target).RemoveElementAttachment(this.Attachment);

            this.Attachment.PositionOffsetChanged -= HandleAttachmentPositionOffsetChange;
            this.Attachment.RotationOffsetChanged -= HandleAttachmentRotationOffsetChange;

            this.Attachment = null;
        }
    }

    /// <summary>
    /// Sends packets to create an elementto a set of players
    /// Do note that the element will be required to have an id assigned for this to work properly
    /// </summary>
    public void CreateFor(IEnumerable<Player> players)
        => AddEntityPacketFactory.CreateAddEntityPacket(new Element[] { this }).SendTo(players);

    /// <summary>
    /// Sends packets to create an elementto a set of players
    /// Do note that the element will be required to have an id assigned for this to work properly
    /// </summary>
    public void CreateFor(Player player)
        => this.CreateFor(new Player[] { player });

    /// <summary>
    /// Sends packets to destroy an elementto a set of players
    /// Do note that the element will be required to have an id assigned for this to work properly
    /// </summary>
    public void DestroyFor(IEnumerable<Player> players)
        => RemoveEntityPacketFactory.CreateRemoveEntityPacket(new Element[] { this }).SendTo(players);

    /// <summary>
    /// Sends packets to destroy an elementto a set of players
    /// Do note that the element will be required to have an id assigned for this to work properly
    /// </summary>
    public void DestroyFor(Player player)
        => this.DestroyFor(new Player[] { player });

    public event ElementChangedEventHandler<Vector3>? PositionChanged;
    public event ElementChangedEventHandler<Vector3>? RotationChanged;
    public event ElementChangedEventHandler<Vector3>? VelocityChanged;
    public event ElementChangedEventHandler<Vector3>? TurnVelocityChanged;
    public event ElementChangedEventHandler<byte>? InteriorChanged;
    public event ElementChangedEventHandler<ushort>? DimensionChanged;
    public event ElementChangedEventHandler<byte>? AlphaChanged;
    public event ElementChangedEventHandler<string>? NameChanged;
    public event ElementChangedEventHandler<bool>? CallPropagationChanged;
    public event ElementChangedEventHandler<bool>? CollisionEnabledChanged;
    public event ElementChangedEventHandler<bool>? FrozenChanged;
    public event ElementChangedEventHandler<Element, ElementId>? IdChanged;
    public event ElementEventHandler<Element, ElementAssociatedWithEventArgs>? AssociatedWith;
    public event ElementEventHandler<Element, ElementAssociatedWithEventArgs>? RemovedFrom;
    public event ElementEventHandler<Element, ElementDataChangedArgs>? DataChanged;
    public event ElementEventHandler<Element, ElementAttachedEventArgs>? Attached;
    public event ElementEventHandler<Element, ElementDetachedEventArgs>? Detached;
    public event ElementEventHandler<Element, ElementAttachOffsetsChangedArgs>? AttachedOffsetChanged;
    public event Action<Element>? Destroyed;

    /// <summary>
    /// Returns a Lua value for the element, this is used for any lua event communication.
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator LuaValue(Element value) => LuaValue.CreateElement((uint)value.Id);

    /// <summary>
    /// Return CancellationToken cancelled when element gets destroyed
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static CancellationToken CreateCancellationToken(Element element)
    {
        if (element.IsDestroyed)
            throw new InvalidOperationException();

        var cancelationTokenSource = new CancellationTokenSource();

        void handleDestroyed(Element element)
        {
            cancelationTokenSource.Cancel();
            element.Destroyed -= handleDestroyed;
        }

        element.Destroyed += handleDestroyed;

        if (element.IsDestroyed)
        {
            cancelationTokenSource.Cancel();
            element.Destroyed -= handleDestroyed;
        }

        return cancelationTokenSource.Token;
    }
}
