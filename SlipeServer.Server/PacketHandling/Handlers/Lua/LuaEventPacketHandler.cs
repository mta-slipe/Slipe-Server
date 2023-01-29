using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Lua;

public class LuaEventPacketHandler : IPacketHandler<LuaEventPacket>
{
    private readonly IElementCollection elementCollection;
    private readonly ILogger logger;
    private readonly MtaServer server;

    public PacketId PacketId => PacketId.PACKET_ID_LUA_EVENT;

    public LuaEventPacketHandler(
        IElementCollection elementCollection,
        ILogger logger,
        MtaServer server
    )
    {
        this.elementCollection = elementCollection;
        this.logger = logger;
        this.server = server;
    }

    public void HandlePacket(IClient client, LuaEventPacket packet)
    {
        var element = this.elementCollection.Get(packet.ElementId);
        if (element == null)
        {
            this.logger.LogWarning($"'{packet.Name}' event triggered on non-existant element {packet.ElementId}");
            return;
        }

        this.server.HandleLuaEvent(new Events.LuaEvent()
        {
            Player = client.Player,
            Name = packet.Name,
            Source = element,
            Parameters = packet.LuaValues.ToArray()
        });
    }
}
