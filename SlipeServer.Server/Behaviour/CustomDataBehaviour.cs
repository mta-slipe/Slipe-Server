using SlipeServer.Packets.Definitions.CustomElementData;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Behaviour
{
    public class CustomDataBehaviour
    {
        public CustomDataBehaviour(MtaServer server, IElementRepository elementRepository)
        {
            server.PlayerJoined += (player) =>
            {
                player.DataChanged += (sender, args) =>
                {
                    LuaValue newValue = args.NewValue;
                    LuaValue? oldValue = args.OldValue;
                    DataSyncType syncType = args.SyncType;
                    string dataName = args.DataName;

                    if (syncType == DataSyncType.Broadcast && !(oldValue is null))
                    {
                        var packet = new CustomDataPacket(sender.Id, dataName, newValue);
                        server.BroadcastPacket(packet);
                    }
                };
            };

        }
    }
}
