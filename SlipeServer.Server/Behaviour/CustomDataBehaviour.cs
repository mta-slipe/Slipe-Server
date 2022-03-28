using SlipeServer.Packets.Definitions.CustomElementData;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.Behaviour;

public class CustomDataBehaviour
{
    private readonly MtaServer server;

    public CustomDataBehaviour(MtaServer server, IElementRepository elementRepository)
    {
        server.ElementCreated += HandleElementCreation;
        this.server = server;
    }

    private void HandleElementCreation(Elements.Element element)
    {
        element.DataChanged += (sender, args) =>
        {
            switch (args.SyncType)
            {
                case DataSyncType.Broadcast:
                    if (args.SyncType == DataSyncType.Broadcast)
                    {
                        var packet = new SetElementDataRpcPacket(sender.Id, args.Key, args.NewValue);
                        this.server.BroadcastPacket(packet);
                    }
                    break;
                case DataSyncType.Subscribe:

                    break;
            }
        };
    }
}

