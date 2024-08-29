using SlipeServer.Packets.Definitions.Pickups;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Handles pickup use and reset events
/// </summary>
public class PickupBehaviour
{
    private readonly MtaServer server;
    private readonly IElementCollection elementCollection;

    public PickupBehaviour(MtaServer server, IElementCollection elementCollection)
    {
        this.server = server;
        this.elementCollection = elementCollection;


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

        var otherPlayers = this.elementCollection
            .GetByType<Player>(ElementType.Player);
        (new PickupHitConfirmPacket(pickup.Id, false, false)).SendTo(otherPlayers);
    }

    private void HandlePickupReset(Pickup pickup, System.EventArgs e)
    {
        this.server.BroadcastPacket(new PickupHideShowPacket(true, [new(pickup.Id, pickup.Model)]));
    }
}
