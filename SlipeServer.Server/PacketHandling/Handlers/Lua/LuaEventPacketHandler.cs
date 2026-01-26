using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Lua;

public class LuaEventPacketHandler(
    IElementCollection elementCollection,
    ILogger logger,
    IMtaServer server
    ) : IPacketHandler<LuaEventPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_LUA_EVENT;

    public void HandlePacket(IClient client, LuaEventPacket packet)
    {
        var element = elementCollection.Get(packet.ElementId);
        if (element == null)
        {
            logger.LogWarning($"'{packet.Name}' event triggered on non-existant element {packet.ElementId}");
            return;
        }

        server.HandleLuaEvent(new Events.LuaEvent()
        {
            Player = client.Player,
            Name = packet.Name,
            Source = element,
            Parameters = packet.LuaValues.ToArray()
        });
    }
}
