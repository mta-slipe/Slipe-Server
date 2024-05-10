using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Extensions;
using System;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for relaying element data changes.
/// </summary>
[Obsolete("It is highly not recommended to use element data!")]
public class CustomDataBehaviour
{
    private readonly MtaServer server;

    public CustomDataBehaviour(MtaServer server)
    {
        server.ElementCreated += HandleElementCreation;
        this.server = server;
    }

    private void HandleElementCreation(Elements.Element element)
    {
        if(element is not ISupportsElementData elementWithCustomDataSupport)
            return;

        var elementData = elementWithCustomDataSupport.ElementData;
        elementData.Changed += (sender, args) =>
        {
            switch (args.SyncType)
            {
                case DataSyncType.Broadcast:
                    var packet = new SetElementDataRpcPacket(sender.Id, args.Key, args.NewValue);
                    this.server.BroadcastPacket(packet);
                    break;
                case DataSyncType.Subscribe:
                    new SetElementDataRpcPacket(sender.Id, args.Key, args.NewValue)
                        .SendTo(elementData.GetPlayersSubcribedToData(args.Key));
                    break;
            }
        };
    }
}

