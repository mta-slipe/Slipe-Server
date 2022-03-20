using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Repositories;
using System.Linq;
using System.Numerics;
using System.Timers;

namespace SlipeServer.Server.Behaviour
{
    public class PedSyncBehaviour
    {
        private readonly IElementRepository elementRepository;
        private readonly Configuration configuration;
        private readonly Timer timer;

        public PedSyncBehaviour(
            MtaServer server,
            IElementRepository elementRepository,
            Configuration configuration)
        {
            this.elementRepository = elementRepository;
            this.configuration = configuration;

            this.timer = new Timer(configuration.SyncIntervals.LightSync)
            {
                AutoReset = true,
            };
            this.timer.Start();
            this.timer.Elapsed += (sender, args) => HandlePedSyncers();

            server.PlayerJoined += HandlePlayerJoin;
        }

        private void HandlePlayerJoin(Player player)
        {
            player.Disconnected += HandlePlayerDisconnect;
        }

        private void HandlePlayerDisconnect(Player player, Elements.Events.PlayerQuitEventArgs e)
        {
            foreach (var ped in player.SyncingPeds.ToArray())
                StopSyncingPed(ped);

            player.Disconnected -= HandlePlayerDisconnect;
        }

        private void HandlePedSyncers()
        {
            var peds = this.elementRepository.GetByType<Ped>(ElementType.Ped)
                .Where(ped => !(ped is Player));

            foreach (var ped in peds)
                UpdatePedSyncer(ped);
        }

        private void UpdatePedSyncer(Ped ped)
        {
            if (!ped.IsSyncable)
                return;

            Player? newSyncer = GetClosestPlayer(ped, this.configuration.PedSyncerDistance);

            if (newSyncer == ped.Syncer)
                return;

            if (ped.Syncer != null)
            {
                StopSyncingPed(ped);
            }

            if (newSyncer != null)
            {
                StartSyncingPed(newSyncer, ped);
            }
        }

        private void StopSyncingPed(Ped ped)
        {
            ped.Syncer?.Client.SendPacket(new PedStopSyncPacket(ped.Id));
            ped.Syncer?.SyncingPeds.Remove(ped);
            ped.Syncer = null;
        }

        private void StartSyncingPed(Player player, Ped ped)
        {
            player.Client.SendPacket(new PedStartSyncPacket(ped.Id, ped.Position, ped.PedRotation, ped.Velocity, ped.Health, ped.Armor));
            player.SyncingPeds.Add(ped);
            ped.Syncer = player;
        }

        private Player? GetClosestPlayer(Ped ped, float maxDistance)
        {
            var players = this.elementRepository
                .GetWithinRange<Player>(ped.Position, maxDistance, ElementType.Player)
                .Where(x => x.Dimension == ped.Dimension)
                .ToArray();

            var nearestDistance = players.Any() ? players.Min(x => Vector3.Distance(x.Position, ped.Position)) : -1;
            return players.Where(x => Vector3.Distance(x.Position, ped.Position) == nearestDistance).FirstOrDefault();
        }
    }
}
