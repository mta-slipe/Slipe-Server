using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements;

public interface IElement
{
    // Core features
    ElementId Id { get; set; }
    ElementType ElementType { get; }
    Element AssociateWith(IMtaServer server);

    void AddChild(IElement element);
    void RemoveChild(IElement element);
    bool IsChildOf(IElement element);


    event Action<Element>? Destroyed;



    // Other features
    byte Alpha { get; set; }
    bool AreCollisionsEnabled { get; set; }
    IEnumerable<Player> AssociatedPlayers { get; }
    IReadOnlyCollection<ElementAssociation> Associations { get; }
    IReadOnlyCollection<Concepts.ElementAttachment> AttachedElements { get; }
    Concepts.ElementAttachment? Attachment { get; }
    CustomData BroadcastableElementData { get; }
    IReadOnlyCollection<IElement> Children { get; }
    ushort Dimension { get; set; }
    Vector3 Forward { get; }
    byte Interior { get; set; }
    bool IsCallPropagationEnabled { get; set; }
    bool IsDestroyed { get; set; }
    bool IsFrozen { get; set; }
    bool IsSync { get; set; }
    bool IsVisibleToEveryone { get; set; }
    string Name { get; set; }
    IElement? Parent { get; set; }
    Vector3 Position { get; set; }
    Vector3 Right { get; }
    Vector3 Rotation { get; set; }
    IEnumerable<Player> Subscribers { get; }
    byte TimeContext { get; }
    byte TimeContextFailureCount { get; set; }
    Vector3 TurnVelocity { get; set; }
    Vector3 Up { get; }
    ElementUpdateContext UpdateContext { get; }
    Vector3 Velocity { get; set; }

    event ElementChangedEventHandler<byte>? AlphaChanged;
    event ElementEventHandler<Element, ElementAssociatedWithEventArgs>? AssociatedWith;
    event ElementEventHandler<Element, ElementAttachedEventArgs>? Attached;
    event ElementEventHandler<Element, ElementAttachOffsetsChangedArgs>? AttachedOffsetChanged;
    event ElementChangedEventHandler<bool>? CallPropagationChanged;
    event ElementChangedEventHandler<bool>? CollisionEnabledChanged;
    event ElementEventHandler<Element, ElementDataChangedArgs>? DataChanged;
    event ElementEventHandler<Element, ElementDetachedEventArgs>? Detached;
    event ElementChangedEventHandler<ushort>? DimensionChanged;
    event ElementChangedEventHandler<bool>? FrozenChanged;
    event ElementChangedEventHandler<Element, ElementId>? IdChanged;
    event ElementChangedEventHandler<byte>? InteriorChanged;
    event ElementChangedEventHandler<string>? NameChanged;
    event ElementChangedEventHandler<Vector3>? PositionChanged;
    event ElementEventHandler<Element, ElementAssociatedWithEventArgs>? RemovedFrom;
    event ElementChangedEventHandler<Vector3>? RotationChanged;
    event ElementChangedEventHandler<Vector3>? TurnVelocityChanged;
    event ElementChangedEventHandler<Vector3>? VelocityChanged;

    void AddSubscriber(Player player);
    Element AssociateWith(Player player);
    Concepts.ElementAttachment AttachTo(Element element, Vector3? positionOffset = null, Vector3? rotationOffset = null);
    bool CanUpdateSync(byte remoteContext);
    void CreateFor(IEnumerable<Player> players);
    void CreateFor(Player player);
    bool Destroy();
    void DestroyFor(IEnumerable<Player> players);
    void DestroyFor(Player player);
    void DetachFrom(Element? element = null);
    byte GetAndIncrementTimeContext();
    LuaValue? GetData(string dataName, bool inherit = false);
    IEnumerable<Player> GetPlayersSubcribedToData(string key);
    bool IsPlayerSubscribedToData(Player player, string key);
    void OverrideTimeContext(byte value);
    void RemoveFrom(IMtaServer server);
    void RemoveFrom(Player player);
    void RemoveSubscriber(Player player);
    void RunAsSync(Action action, bool value = true);
    Task RunAsSync(Func<Task> action, bool value = true);
    void RunWithContext(Action action, ElementUpdateContext context);
    Task RunWithContext(Func<Task> action, ElementUpdateContext context);
    void SetData(string key, LuaValue value, DataSyncType syncType = DataSyncType.Local);
    void SubscribeToData(Player player, string key);
    void UnsubscribeFromAllData(Player player);
    void UnsubscribeFromData(Player player, string key);
}
