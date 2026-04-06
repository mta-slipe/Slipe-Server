using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;

namespace SlipeServer.Scripting.Definitions;

public class SatchelScriptDefinitions(IMtaServer server)
{
    [ScriptFunctionDefinition("detonateSatchels")]
    public bool DetonateSatchels(Player player)
    {
        var packet = new DetonateSatchelsPacket(player.Id, (ushort)player.Client.Ping);
        packet.SendTo(server.Players);
        return true;
    }
}
