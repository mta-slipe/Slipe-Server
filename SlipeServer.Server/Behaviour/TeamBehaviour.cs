using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.Elements.Events;

namespace SlipeServer.Server.Behaviour
{
    public class TeamBehaviour
    {
        public TeamBehaviour(MtaServer server)
        {
            server.PlayerJoined += (player) =>
            {
                player.TeamChanged += (sender, args) =>
                {
                    var packet = new SetPlayerTeamRpcPacket() {
                        SourceElementId = sender.Id,
                        TeamId = args.NewTeam.Id
                    };

                    server.BroadcastPacket(packet);
                };
            };
        }
    }
}
