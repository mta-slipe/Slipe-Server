using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Constants;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using System;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Rpc;

public class RpcPacketHandler : IPacketHandler<RpcPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_RPC;

    private readonly IElementCollection elementCollection;
    private readonly Configuration configuration;
    private readonly ILogger logger;
    private readonly MtaServer server;
    private readonly RootElement root;

    public RpcPacketHandler(
        ILogger logger,
        MtaServer server,
        RootElement root,
        IElementCollection elementCollection,
        Configuration configuration
    )
    {
        this.logger = logger;
        this.server = server;
        this.root = root;
        this.elementCollection = elementCollection;
        this.configuration = configuration;
    }

    public void HandlePacket(IClient client, RpcPacket packet)
    {
        switch (packet.FunctionId)
        {
            case RpcFunctions.INITIAL_DATA_STREAM:
                HandleDataStream(client);
                break;

            case RpcFunctions.PLAYER_INGAME_NOTICE:
                HandleIngameNotice(client);
                break;

            case RpcFunctions.PLAYER_WEAPON:
                HandlePlayerWeapon(client, packet);
                break;

            case RpcFunctions.PLAYER_TARGET:
                HandlePlayerTarget(client, packet);
                break;

            case RpcFunctions.KEY_BIND:
                HandlePlayerBindKey(client, packet);
                break;

            case RpcFunctions.CURSOR_EVENT:
                HandlePlayerCursorEvent(client, packet);
                break;

            default:
                this.logger.LogWarning("Received RPC of type {type}", packet.FunctionId);
                break;
        }
    }

    private void HandleDataStream(IClient client)
    {
        var players = this.elementCollection.GetByType<Elements.Player>(ElementType.Player);

        var otherPlayers = players
            .Except([client.Player])
            .ToArray();

        var existingPlayersListPacket = PlayerPacketFactory.CreatePlayerListPacket(otherPlayers, true);
        client.SendPacket(existingPlayersListPacket);

        var elements = this.elementCollection
            .GetAll()
            .Where(x => x.Associations.ToArray().Any(y => y.IsGlobal))
            .Where(x => x.IsVisibleToEveryone);

        var packet = AddEntityPacketFactory.CreateAddEntityPacket(elements);
        client.SendPacket(packet);

        using (var scope = new ClientPacketScope(client.Player))
        {
            foreach (var player in otherPlayers)
                if (player.Vehicle != null && player.Seat.HasValue)
                    player.WarpIntoVehicle(player.Vehicle, player.Seat.Value);
        }

        var newPlayerListPacket = PlayerPacketFactory.CreatePlayerListPacket([client.Player], false);
        newPlayerListPacket.SendTo(otherPlayers);

        SyncPacketFactory.CreateSyncSettingsPacket(this.configuration).SendTo(client.Player);
        SyncPacketFactory.CreateSyncIntervalPacket(this.configuration).SendTo(client.Player);

        foreach (var player in otherPlayers)
        {
            if (player.GetAllStats().Count > 0)
                PedPacketFactory.CreatePlayerStatsPacket(player).SendTo(client.Player);

            if (player.GetAllStats().Count > 0)
                PedPacketFactory.CreateFullClothesPacket(player).SendTo(client.Player);
        }

        this.server.HandlePlayerJoin(client.Player);
    }

    private void HandleIngameNotice(IClient client)
    {
        var players = this.elementCollection.GetByType<Elements.Player>(ElementType.Player);

        var isVersionValid = Version.TryParse(string.Join(".", client.Version!.Replace("-", ".").Split(".").Take(4)), out Version? result);
        if (!isVersionValid)
        {
            client.Player.Kick(PlayerDisconnectType.BAD_VERSION);
            return;
        }

        if (result < this.configuration.MinVersion)
        {
            client.SendPacket(PlayerPacketFactory.CreateUpdateInfoPacket(this.configuration.MinVersion));
            client.Player.Kick($"Disconnected: Minimum mta version required: {this.configuration.MinVersion}");
            return;
        }

        client.SendPacket(new JoinedGamePacket(
            client.Player.Id,
            players.Count() + 1,
            this.root.Id,
            this.configuration.HttpUrl != null ? HttpDownloadType.HTTP_DOWNLOAD_ENABLED_URL : HttpDownloadType.HTTP_DOWNLOAD_ENABLED_PORT,
            this.configuration.HttpPort,
            this.configuration.HttpUrl ?? "",
            this.configuration.HttpConnectionsPerClient,
            1,
            isVoiceEnabled: this.configuration.IsVoiceEnabled
        ));
    }

    private void HandlePlayerWeapon(IClient client, RpcPacket packet)
    {
        lock (client.Player.CurrentWeaponLock)
        {
            var previousSlot = client.Player.CurrentWeaponSlot;
            if (packet.Reader.GetBit() && client.Player.CurrentWeapon != null && (
                previousSlot == WeaponSlot.Projectiles ||
                previousSlot == WeaponSlot.HeavyWeapons ||
                previousSlot == WeaponSlot.Special1
            ))
            {
                client.Player.CurrentWeapon.Ammo = 0;
                client.Player.CurrentWeapon.AmmoInClip = 0;
            }

            var slot = packet.Reader.GetWeaponSlot();
            client.Player.CurrentWeaponSlot = (WeaponSlot)slot;

            if (WeaponConstants.SlotsWithAmmo.Contains(slot) && client.Player.CurrentWeapon != null)
            {
                (var ammo, var inClip) = packet.Reader.GetAmmoTuple(true);
                client.Player.CurrentWeapon.Ammo = ammo;
                client.Player.CurrentWeapon.AmmoInClip = inClip;
                client.Player.TriggerWeaponAmmoUpdate(client.Player.CurrentWeapon.Type, ammo, inClip);
            }
        }
    }

    private void HandlePlayerTarget(IClient client, RpcPacket packet)
    {
        var id = packet.Reader.GetElementId();
        var element = this.elementCollection.Get(id);
        client.Player.Target = element;
    }

    private void HandlePlayerBindKey(IClient client, RpcPacket packet)
    {
        var type = packet.Reader.GetBit() ? BindType.ControlFunction : BindType.Function;
        var state = packet.Reader.GetBit() ? KeyState.Down : KeyState.Up;
        var size = (packet.Reader.Size - packet.Reader.Counter) >> 3;
        var key = packet.Reader.GetStringCharacters(size);
        client.Player.TriggerBoundKey(type, state, key);
    }

    private void HandlePlayerCursorEvent(IClient client, RpcPacket packet)
    {
        var button = packet.Reader.GetByteCapped(3);
        var x = packet.Reader.GetCompressedUint16();
        var y = packet.Reader.GetCompressedUint16();
        var worldPosition = packet.Reader.GetVector3WithZAsFloat();
        Element? element = null;
        if (packet.Reader.GetBit())
            element = this.elementCollection.Get(packet.Reader.GetElementId());

        client.Player.TriggerCursorClicked(button, new(x, y), worldPosition, element);
    }
}
