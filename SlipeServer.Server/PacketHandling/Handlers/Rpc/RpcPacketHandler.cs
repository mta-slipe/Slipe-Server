using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Constants;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;
using System;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Rpc
{
    public class RpcPacketHandler : IPacketHandler<RpcPacket>
    {
        public PacketId PacketId => PacketId.PACKET_ID_RPC;

        private readonly IElementRepository elementRepository;
        private readonly Configuration configuration;
        private readonly ILogger logger;
        private readonly MtaServer server;
        private readonly RootElement root;


        public RpcPacketHandler(
            ILogger logger,
            MtaServer server,
            RootElement root,
            IElementRepository elementRepository,
            Configuration configuration
        )
        {
            this.logger = logger;
            this.server = server;
            this.root = root;
            this.elementRepository = elementRepository;
            this.configuration = configuration;
        }

        public void HandlePacket(Client client, RpcPacket packet)
        {
            switch (packet.FunctionId)
            {
                case RpcFunctions.PLAYER_INGAME_NOTICE:
                    HandleIngameNotice(client);
                    break;

                case RpcFunctions.PLAYER_WEAPON:
                    HandlePlayerWeapon(client, packet);
                    break;
                    
                case RpcFunctions.PLAYER_TARGET:
                    HandlePlayerTarget(client, packet);
                    break;

                default:
                    this.logger.LogWarning("Received RPC of type {type}", packet.FunctionId);
                    break;
            }
        }

        private void HandleIngameNotice(Client client)
        {
            var players = this.elementRepository.GetByType<Elements.Player>(ElementType.Player);

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

            var otherPlayers = players
                .Except(new Elements.Player[] { client.Player })
                .ToArray();

            var existingPlayersListPacket = PlayerPacketFactory.CreatePlayerListPacket(otherPlayers, true);
            client.SendPacket(existingPlayersListPacket);

            var newPlayerListPacket = PlayerPacketFactory.CreatePlayerListPacket(new Elements.Player[] { client.Player }, false);
            newPlayerListPacket.SendTo(otherPlayers);

            SyncPacketFactory.CreateSyncSettingsPacket(this.configuration).SendTo(client.Player);
            SyncPacketFactory.CreateSyncIntervalPacket(this.configuration).SendTo(client.Player);

            foreach (var player in otherPlayers)
            {
                if (player.GetAllStats().Count > 0)
                    PedPacketFactory.CreatePlayerStatsPacket(player).SendTo(client.Player);
            }

            this.server.HandlePlayerJoin(client.Player);
        }

        private void HandlePlayerWeapon(Client client, RpcPacket packet)
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
                }
            }
        }

        private void HandlePlayerTarget(Client client, RpcPacket packet)
        {
            uint id = packet.Reader.GetElementId();
            Element? element = this.elementRepository.Get(id);
            client.Player.Target = element;
        }
    }
}
