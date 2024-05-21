using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.CustomElementData;
using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Definitions.Transgression;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Packets.Rpc;
using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SlipeServer.Server.Debugging.PacketRecording;

public abstract class PacketRecorder : IDisposable
{
    private readonly Player player;
    private readonly ClientRecording clientRecording;
    private readonly ImmutableArray<PacketHandlerRegistration> packetHandlersRegistrations;

    public PacketRecorder(Player player, MtaServer mtaServer)
    {
        var client = player.Client;
        this.player = player;
        this.clientRecording = new ClientRecording(player);
        this.player.Client = this.clientRecording;

        this.clientRecording.PacketSent += HandleOutgoingPacketSent;

        var packetHandlersRegistrations = new List<PacketHandlerRegistration>
        {
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_SERVER_JOINEDGAME, new PacketRecorderHandler<JoinedGamePacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_JOINDATA, new PacketRecorderHandler<PlayerJoinDataPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_QUIT, new PacketRecorderHandler<PlayerQuitPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_TIMEOUT, new PacketRecorderHandler<PlayerTimeoutPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, new PacketRecorderHandler<PlayerPureSyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_KEYSYNC, new PacketRecorderHandler<KeySyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_CAMERA_SYNC, new PacketRecorderHandler<CameraSyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_WEAPON_BULLETSYNC, new PacketRecorderHandler<WeaponBulletSyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_BULLETSYNC, new PacketRecorderHandler<PlayerBulletSyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PROJECTILE, new PacketRecorderHandler<ProjectileSyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_EXPLOSION, new PacketRecorderHandler<ExplosionPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_DETONATE_SATCHELS, new PacketRecorderHandler<DetonateSatchelsPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_DESTROY_SATCHELS, new PacketRecorderHandler<DestroySatchelsPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_RPC, new PacketRecorderHandler<RpcPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_LUA_EVENT, new PacketRecorderHandler<LuaEventPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_ACINFO, new PacketRecorderHandler<PlayerACInfoPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_DIAGNOSTIC, new PacketRecorderHandler<PlayerDiagnosticPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_MODINFO, new PacketRecorderHandler<PlayerModInfoPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_NETWORK_STATUS, new PacketRecorderHandler<PlayerNetworkStatusPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_SCREENSHOT, new PacketRecorderHandler<PlayerScreenshotPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_WASTED, new PacketRecorderHandler<PlayerWastedPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_VEHICLE_INOUT, new PacketRecorderHandler<VehicleInOutPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_VEHICLE_PURESYNC, new PacketRecorderHandler<VehiclePureSyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_VEHICLE_DAMAGE_SYNC, new PacketRecorderHandler<VehicleDamageSyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_UNOCCUPIED_VEHICLE_SYNC, new PacketRecorderHandler<UnoccupiedVehicleSyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_VEHICLE_PUSH_SYNC, new PacketRecorderHandler<VehiclePushSyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_VOICE_DATA, new PacketRecorderHandler<VoiceDataPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_VOICE_END, new PacketRecorderHandler<VoiceEndPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_TRANSGRESSION, new PacketRecorderHandler<TransgressionPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PED_SYNC, new PacketRecorderHandler<PedSyncPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PED_TASK, new PacketRecorderHandler<PedTaskPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_PED_WASTED, new PacketRecorderHandler<PedWastedPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_LATENT_TRANSFER, new PacketRecorderHandler<LatentLuaEventPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_CUSTOM_DATA, new PacketRecorderHandler<CustomDataPacket>(client, HandleIncomingPacketReceived)),
            mtaServer.RegisterPacketHandler(PacketId.PACKET_ID_VEHICLE_TRAILER, new PacketRecorderHandler<VehicleTrailerSyncPacket>(client, HandleIncomingPacketReceived)),
        };
        this.packetHandlersRegistrations = [.. packetHandlersRegistrations];
    }

    protected void HandleOutgoingPacketSent(Packet packet)
    {
        HandlePacketSent(packet, PacketDirection.Outgoing);
    }

    protected void HandleIncomingPacketReceived(Packet packet)
    {
        HandlePacketSent(packet, PacketDirection.Incoming);
    }

    protected abstract void HandlePacketSent(Packet packet, PacketDirection packetDirection);

    public virtual void Dispose()
    {
        foreach (var packetHandlerRegistration in packetHandlersRegistrations)
        {
            packetHandlerRegistration.Unregister();
        }
        this.player.Client = this.clientRecording.Client;
    }
}
