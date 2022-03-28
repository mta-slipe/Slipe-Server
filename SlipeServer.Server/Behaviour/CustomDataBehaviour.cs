using SlipeServer.Packets.Definitions.CustomElementData;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.Behaviour;

public class CustomDataBehaviour
{
    private readonly MtaServer server;

    public CustomDataBehaviour(MtaServer server, IElementRepository elementRepository)
    {
        server.PlayerJoined += HandlePlayerJoin;
        this.server = server;
    }

    private void HandlePlayerJoin(Elements.Player player)
    {
        player.DataChanged += (sender, args) =>
        {
            switch (args.SyncType)
            {
                case DataSyncType.Broadcast:
                    if (args.SyncType == DataSyncType.Broadcast)
                    {
                        var packet = new CustomDataPacket(sender.Id, args.Key, args.NewValue);
                        this.server.BroadcastPacket(packet);
                    }
                    break;
                case DataSyncType.Subscribe:

                    break;
            }
        };
    }
}

