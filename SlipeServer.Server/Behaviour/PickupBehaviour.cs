using SlipeServer.Packets.Definitions.Pickups;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.Behaviour
{
    public class PickupBehaviour
    {
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;

        public PickupBehaviour(MtaServer server, IElementRepository elementRepository)
        {
            this.server = server;
            this.elementRepository = elementRepository;


            server.ElementCreated += HandleElementCreation;
        }

        private void HandleElementCreation(Element element)
        {
            if (element is Pickup pickup)
            {
                pickup.Used += HandlePickupUsed;
                pickup.Reset += HandlePickupReset;
            }
        }

        private void HandlePickupUsed(Pickup pickup, Elements.Events.PickupUsedEventArgs e)
        {
            (new PickupHitConfirmPacket(pickup.Id, false, true)).SendTo(e.Player);

            var otherPlayers = this.elementRepository
                .GetByType<Player>(ElementType.Player);
            (new PickupHitConfirmPacket(pickup.Id, false, false)).SendTo(otherPlayers);
        }

        private void HandlePickupReset(Pickup pickup, System.EventArgs e)
        {
            this.server.BroadcastPacket(new PickupHideShowPacket(true, new PickupIdAndModel[] { new(pickup.Id, pickup.Model) }));
        }
    }
}
