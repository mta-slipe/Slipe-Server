using SlipeServer.Packets.Definitions.Pickups;
using SlipeServer.Scripting;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;

namespace SlipeServer.DropInReplacement.PacketHandlers;

/// <summary>
/// Replaces <see cref="SlipeServer.Server.Behaviour.PickupBehaviour"/>.
/// Resets the scripting cancel state before <see cref="Pickup.BeforeUsed"/> fires so that
/// <c>cancelEvent()</c> inside <c>onPickupUse</c> prevents the pickup effects from applying.
/// Sends <see cref="PickupHitConfirmPacket"/> on <see cref="Pickup.Used"/> exactly as the
/// default behaviour does (only reached when the event was NOT cancelled).
/// </summary>
public class ScriptingPickupBehaviour
{
    private readonly IMtaServer server;
    private readonly IElementCollection elementCollection;
    private readonly IScriptEventRuntime eventRuntime;

    public ScriptingPickupBehaviour(IMtaServer server, IElementCollection elementCollection, IScriptEventRuntime eventRuntime)
    {
        this.server = server;
        this.elementCollection = elementCollection;
        this.eventRuntime = eventRuntime;

        server.ElementCreated += HandleElementCreation;
    }

    private void HandleElementCreation(Element element)
    {
        if (element is Pickup pickup)
        {
            // Subscribe to BeforeUsed FIRST (before any Lua addEventHandler runs) so the
            // cancel-state reset happens before the onPickupUse scripting proxy executes.
            pickup.BeforeUsed += HandlePickupBeforeUsed;
            pickup.Used += HandlePickupUsed;
            pickup.Reset += HandlePickupReset;
        }
    }

    private void HandlePickupBeforeUsed(Pickup pickup, CancellablePickupUseEventArgs e)
    {
        this.eventRuntime.CancelEvent(false);
    }

    private void HandlePickupUsed(Pickup pickup, PickupUsedEventArgs e)
    {
        (new PickupHitConfirmPacket(pickup.Id, false, true)).SendTo(e.Player);

        var otherPlayers = this.elementCollection.GetByType<Player>(ElementType.Player);
        (new PickupHitConfirmPacket(pickup.Id, false, false)).SendTo(otherPlayers);
    }

    private void HandlePickupReset(Pickup pickup, System.EventArgs e)
    {
        this.server.BroadcastPacket(new PickupHideShowPacket(true, [new(pickup.Id, pickup.Model)]));
    }
}
