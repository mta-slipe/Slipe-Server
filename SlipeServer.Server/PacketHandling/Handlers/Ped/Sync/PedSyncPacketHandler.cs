using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class PedSyncPacketHandler : IPacketHandler<PedSyncPacket>
    {
        private readonly ILogger logger;
        private readonly IElementRepository elementRepository;
        private readonly Configuration configuration;
        private readonly Timer pulseTimer;

        public PacketId PacketId => PacketId.PACKET_ID_PED_SYNC;

        public PedSyncPacketHandler(ILogger logger, IElementRepository elementRepository, Configuration configuration)
        {
            this.logger = logger;
            this.elementRepository = elementRepository;
            this.configuration = configuration;
            this.pulseTimer = new Timer() {
                AutoReset = true,
                Enabled = true,
                Interval = 500
            };
            this.pulseTimer.Elapsed += Update;
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

        private void Update(object sender, ElapsedEventArgs args)
        {
            IEnumerable<Ped> allPeds = this.elementRepository.GetByType<Ped>(ElementType.Ped).Where(ped =>
                !(ped is Player)).ToArray();

            foreach (var ped in allPeds)
            {
                UpdatePed(ped);
            }
        }

        private void UpdatePed(Ped ped)
        {
            Player? syncer = ped.Syncer;

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
                    if (Vector3.Distance(syncer.Position, ped.Position) < this.configuration.SyncIntervals.PedSyncerDistance || (ped.Dimension != syncer.Dimension))
                    {
                        StopSync(ped);

                        if (ped != null)
                        {
                            FindSyncer(ped);
                        }
                    } else
                    {
                        FindSyncer(ped);
                    }
                }
            }
        }

        private void FindSyncer(Ped ped)
        {
            if (ped.IsSyncable)
            {
                Player? player = FindPlayerCloseToPed(ped, this.configuration.SyncIntervals.PedSyncerDistance);
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
                player.Client.SendPacket(new PedStartSyncPacket(ped.Id));

                ped.Syncer = player;
                
                player.SyncingPeds.Add(ped);
            }
        }

        private void StopSync(Ped ped)
        {
            var syncer = ped.Syncer;
            syncer?.Client.SendPacket(new PedStopSyncPacket(ped.Id));

            if (syncer != null)
                syncer.SyncingPeds.Remove(ped);

            ped.Syncer = null;
        }

        private Player? FindPlayerCloseToPed(Ped ped, float maxDistance)
        {
            Player? lastPlayerSyncing = null;

            var allPlayers = this.elementRepository
                .GetWithinRange<Player>(ped.Position, maxDistance, ElementType.Player)
                .Where(x => x.Dimension == ped.Dimension)
                .ToArray();

            foreach (Player thePlayer in allPlayers)
            {
                if (lastPlayerSyncing != null ||
                    thePlayer.SyncingPeds.Count < lastPlayerSyncing?.SyncingPeds.Count)
                {
                        lastPlayerSyncing = thePlayer;
                }
            }

            return lastPlayerSyncing;
        }

        public void HandlePacket(Client client, PedSyncPacket packet)
        {
            foreach (var syncData in packet.Syncs)
            {
                Ped pedElement = (Ped)this.elementRepository.Get(syncData.SourceElementId)!;

                if ((pedElement != null) && !(pedElement is Ped))
                {
                    pedElement.RunAsSync(() =>
                    {
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

                        }

                        var players = this.elementRepository.GetByType<Player>(ElementType.Player)
                            .Where(player => player.Client != client);

                        packet.SendTo(players);
                    });
                }
            }
        }
    }
}
