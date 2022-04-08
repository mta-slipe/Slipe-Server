using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

namespace SlipeServer.Server.Behaviour;

public class TeamBehaviour
{
    public TeamBehaviour(MtaServer server)
    {
        server.PlayerJoined += (player) =>
        {
            player.TeamChanged += (sender, args) =>
            {
                var packet = new SetPlayerTeamRpcPacket()
                {
                    SourceElementId = sender.Id,
                    TeamId = args.NewTeam?.Id ?? 0
                };
                server.BroadcastPacket(packet);
            };
        };
    }
}
