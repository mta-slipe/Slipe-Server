using System;

namespace SlipeServer.Server.ServerBuilders;

[Flags]
public enum ServerBuilderDefaultPacketHandlers : ulong
{
    None = 0x00,

    JoinedGamePacketHandler = 0x01,
    JoinDataPacketHandler = 0x02,
    PlayerQuitPacketHandler = 0x04,
    PlayerTimeoutPacketHandler = 0x08,

    PlayerPureSyncPacketHandler = 0x10,
    KeySyncPacketHandler = 0x20,
    CameraSyncPacketHandler = 0x40,

    WeaponBulletSyncPacketHandler = 0x80,
    PlayerBulletSyncPacketHandler = 0x100,

    ProjectileSyncPacketHandler = 0x200,

    ExplosionPacketHandler = 0x400,

    CommandPacketHandler = 0x800,

    DetonateSatchelsPacketHandler = 0x1000,
    DestroySatchelsPacketHandler = 0x2000,

    RpcPacketHandler = 0x4000,

    LuaEventPacketHandler = 0x8000,

    PlayerAcInfoPacketHandler = 0x10000,
    PlayerDiagnosticPacketHandler = 0x20000,
    PlayerModInfoPacketHandler = 0x40000,
    PlayerNetworkStatusPacketHandler = 0x80000,
    PlayerScreenshotPacketHandler = 0x100000,
    PlayerWastedPacketHandler = 0x200000,

    VehicleInOutPacketHandler = 0x400000,
    VehiclePureSyncPacketHandler = 0x800000,
    VehicleDamageSyncPacketHandler = 0x1000000,
    UnoccupiedVehicleSyncPacketHandler = 0x2000000,
    VehiclePushSyncPacketHandler = 0x4000000,

    VoiceDataPacketHandler = 0x8000000,
    VoiceEndPacketHandler = 0x10000000,
    TransgressionPacketHandler = 0x20000000,
    PedSyncPacketHandler = 0x40000000,
    PedTaskPacketHandler = 0x80000000,
    PedWastedPacketHandler = 0x100000000,
    LatentLuaEventPacketHandler = 0x200000000,
    CustomDataPacketHandler = 0x400000000,
    VehicleTrailerSyncPacketHandler = 0x800000000,
    PlayerResourceStartedPacketHandler = 0x1000000000,

    All = 0xFFFFFFFFFFFFFFFF
}

[Flags]
public enum ServerBuilderDefaultBehaviours : ulong
{
    None = 0x00,

    AseBehaviour = 0x01,
    LocalServerAnnouncementBehaviour = 0x02,
    MasterServerAnnouncementBehaviour = 0x04,

    EventLoggingBehaviour = 0x08,
    VelocityBehaviour = 0x10,
    DefaultChatBehaviour = 0x20,
    NicknameChangeBehaviour = 0x40,
    CollisionShapeBehaviour = 0x80,

    PlayerJoinElementBehaviour = 0x100,

    ElementPacketBehaviour = 0x200,
    PedPacketBehaviour = 0x400,
    PlayerPacketBehaviour = 0x800,
    VehicleWarpBehaviour = 0x1000,
    VehicleRespawnBehaviour = 0x2000,
    VehicleBehaviour = 0x4000,
    VoiceBehaviour = 0x8000,
    LightSyncBehaviour = 0x10000,
    TeamBehaviour = 0x20000,
    RadarAreaBehaviour = 0x40000,
    BlipBehaviour = 0x80000,
    ObjectPacketBehaviour = 0x100000,
    PickupBehaviour = 0x200000,
    MarkerBehaviour = 0x400000,
    MapInfoBehaviour = 0x800000,
    PedSyncBehaviour = 0x1000000,
    UnoccupiedVehicleSyncBehaviour = 0x2000000,
    CustomDataBehaviour = 0x4000000,
    PlayerBehaviour = 0x8000000,

    All = 0xFFFFFFFFFFFFFFFF
}

[Flags]
public enum ServerBuilderDefaultMiddleware : ulong
{
    None = 0x00,

    ProjectileSyncPacketMiddleware = 0x01,
    DetonateSatchelsPacketMiddleware = 0x02,
    DestroySatchelsPacketMiddleware = 0x04,
    ExplosionPacketMiddleware = 0x08,
    PlayerPureSyncPacketMiddleware = 0x10,
    KeySyncPacketMiddleware = 0x20,
    LightSyncBehaviourMiddleware = 0x40,

    All = 0xFFFFFFFFFFFFFFFF
}

[Flags]
public enum ServerBuilderDefaultServices : ulong
{
    None = 0x00,

    All = 0xFFFFFFFFFFFFFFFF
}


[Flags]
public enum ServerBuilderDefaultElementRelayers : ulong
{
    None = 0x00,
    ElementPosition = 0x01,
    ElementRotation = 0x02,
    ElementAlpha = 0x04,
    ElementDimension = 0x08,
    ElementInterior = 0x10,
    ElementIsCallPropagationEnabled = 0x20,
    ElementAreCollisionsEnabled = 0x40,
    ElementIsFrozenChanged = 0x80,
    ElementDestroyed = 0x100,
    ElementAttached = 0x200,
    ElementDetached = 0x400,
    ElementAttachmentOffsetChanged = 0x800,

    All = 0xFFFFFFFFFFFFFFFF
}


[Flags]
public enum ServerBuilderDefaultBlipRelayers : ulong
{
    None = 0x00,
    BlipColorChanged = 0x1,
    BlipSizeChanged = 0x2,
    BlipIconChanged = 0x4,
    BlipVisibleDistanceChanged = 0x8,
    BlipOrderingChanged = 0x10,

    All = 0xFFFFFFFFFFFFFFFF
}


[Flags]
public enum ServerBuilderDefaultMarkerRelayers : ulong
{
    None = 0x00,

    MarkerTypeChanged = 0x1,
    MarkerIconChanged = 0x2,
    MarkerSizeChanged = 0x4,
    MarkerColorChanged = 0x8,
    MarkerTargetPositionChanged = 0x10,

    All = 0xFFFFFFFFFFFFFFFF
}


public struct ServerBuilderDefaultRelayers
{
    public ServerBuilderDefaultElementRelayers Element { get; set; }
    public ServerBuilderDefaultBlipRelayers Blip { get; set; }
    public ServerBuilderDefaultMarkerRelayers Marker { get; set; }

    public static ServerBuilderDefaultRelayers None => new()
    {
        Element = ServerBuilderDefaultElementRelayers.None,
        Blip = ServerBuilderDefaultBlipRelayers.None,
        Marker = ServerBuilderDefaultMarkerRelayers.None,
    };

    public static ServerBuilderDefaultRelayers All => new()
    {
        Element = ServerBuilderDefaultElementRelayers.All,
        Blip = ServerBuilderDefaultBlipRelayers.All,
        Marker = ServerBuilderDefaultMarkerRelayers.All,
    };
}
