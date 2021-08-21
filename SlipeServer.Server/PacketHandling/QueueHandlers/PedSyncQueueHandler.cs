using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class PedSyncQueueHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger _logger;
        private readonly IElementRepository _elementRepository;
        private readonly Configuration _configuration;
        private readonly MtaServer _server;

        public override IEnumerable<PacketId> SupportedPacketIds => this.PacketTypes.Keys;
        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>() {
            [PacketId.PACKET_ID_PED_SYNC] = typeof(PedSyncPacket),
            [PacketId.PACKET_ID_PED_TASK] = typeof(PedTaskPacket),
            [PacketId.PACKET_ID_PED_WASTED] = typeof(PedWastedPacket),
            [PacketId.PACKET_ID_PED_STARTSYNC] = typeof(PedStartSyncPacket),
            [PacketId.PACKET_ID_PED_STOPSYNC] = typeof(PedStopSyncPacket),
        };

        public PedSyncQueueHandler(ILogger logger, IElementRepository elementRepository, MtaServer server, Configuration configuration, int sleepInterval = 10, int workerCount = 1) : base(sleepInterval, workerCount)
        {
            this._logger = logger;
            this._elementRepository = elementRepository;
            this._configuration = configuration;
            this._server = server;
        }

        protected override void HandlePacket(Client client, Packet packet)
        {
            try
            {
                switch (packet)
                {
                    case PedSyncPacket pedSyncPacket:
                        this.HandlePedSyncPacket(client, pedSyncPacket);
                        break;
                }
            }
            catch (Exception e)
            {
                this._logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
        }

        private void OverrideSyncer(Ped ped, Player player)
        {
            var currentSyncer = ped.Syncer;
            if (currentSyncer != null)
            {
                if (currentSyncer != player)
                {
                    StopSync(ped);
                }
            }

            if (player != null && ped != null)
            {
                StartSync(player, ped);
            }
        }

        private void Update()
        {
            IEnumerable<Ped> allPeds = this._elementRepository.GetByType<Ped>(ElementType.Ped).Where(ped =>
                !(ped is Player)).ToArray();

            foreach (var ped in allPeds)
            {
                UpdatePed(ped);
            }
        }

        private void UpdatePed(Ped ped)
        {
            Player syncer = ped.Syncer;

            if (!ped.IsSyncable)
            {
                if (syncer != null)
                {
                    StopSync(ped);
                }
            } else
            {
                if (syncer != null)
                {
                    if (!syncer.Position.IsNearAnotherPoint3D(ped.Position,
                        this._configuration.SyncIntervals.PedSyncerDistance) || (ped.Dimension != syncer.Dimension))
                    {
                        // Stop syncer from syncing it
                        StopSync(ped);

                        if (ped != null)
                        {
                            // Find new sycer for it
                            FindSyncer(ped);
                        }
                    } else
                    {
                        // Try to find a syncer for it
                        FindSyncer(ped);
                    }
                }
            }
        }

        private void FindSyncer(Ped ped)
        {
            if (ped.IsSyncable)
            {
                // Find a player close enough to him
                Player player = FindPlayerCloseToPed(ped, _configuration.SyncIntervals.PedSyncerDistance);
                if (player != null)
                {
                    StartSync(player, ped);
                }
            }
        }

        private void StartSync(Player player, Ped ped)
        {
            if (ped.IsSyncable)
            {
                // Tell the player
                player.Client.SendPacket(new PedStartSyncPacket(ped.Id));

                // Mark player as the syncing player
                ped.Syncer = player;
            }
        }

        private void StopSync(Ped ped)
        {
            var syncer = ped.Syncer;
            syncer?.Client.SendPacket(new PedStopSyncPacket(ped.Id));

            ped.Syncer = null;
        }

        private Player FindPlayerCloseToPed(Ped ped, float maxDistance)
        {
            var pedPosition = ped.Position;

            Player? lastPlayerSyncing = null;
            Player? player;

            var allPlayers = this._elementRepository.GetByType<Player>(ElementType.Player)
                .ToArray();

            foreach (Player thePlayer in allPlayers)
            {
                if (ped.Position.IsNearAnotherPoint3D(thePlayer.Position, maxDistance))
                {
                    if (thePlayer.Dimension == ped.Dimension)
                    {
                        if (lastPlayerSyncing != null ||
                            thePlayer.SyncingPeds.Count < lastPlayerSyncing?.SyncingPeds.Count)
                        {
                            lastPlayerSyncing = thePlayer;
                        }
                    }
                }
            }

            return lastPlayerSyncing;
        }

        private void HandlePedSyncPacket(Client client, PedSyncPacket packet)
        {
            foreach (var syncData in packet.Syncs)
            {
                Ped pedElement = (Ped)this._elementRepository.Get(syncData.SourceElementId);

                if ((pedElement != null) && !(pedElement is Ped))
                {

                    // Is the player syncing this ped?
                    // Verify context matches
                    if (pedElement.Syncer?.Client == client && pedElement.CanUpdateSync(syncData.TimeSyncContext))
                    {
                        if ((syncData.Flags & 0x01) != 0)
                        {
                            pedElement.Position = syncData.Position;
                        }
                        if ((syncData.Flags & 0x02) != 0)
                        {
                            pedElement.PedRotation = syncData.Rotation;
                        }
                        if ((syncData.Flags & 0x04) != 0)
                        {
                            pedElement.Velocity = syncData.Velocity;
                        }
                        if ((syncData.Flags & 0x08) != 0)
                        {
                            float previousHealth = pedElement.Health;
                            pedElement.Health = syncData.Health;
                        }

                        if ((syncData.Flags & 0x10) != 0)
                        {
                            pedElement.Armor = syncData.Armor;
                        }

                        if ((syncData.Flags & 0x20) != 0)
                        {
                            pedElement.IsOnFire = syncData.IsOnFire;
                        }

                        if ((syncData.Flags & 0x40) != 0)
                        {
                            pedElement.IsInWater = syncData.IsInWater;
                        }

                        // Send this sync
                        // syncData.Send = true;
                    }

                    var players = this._elementRepository.GetByType<Player>(ElementType.Player)
                        .Where(player => player.Client != client);

                    packet.SendTo(players);
                }
            }
        }

    }
}
