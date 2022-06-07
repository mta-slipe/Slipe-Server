using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers.Connection;

public class JoinedGamePacketHandler : IPacketHandler<JoinedGamePacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_JOIN;

    private readonly ushort bitStreamVersion;

    public JoinedGamePacketHandler(
        Configuration configuration
    )
    {
        this.bitStreamVersion = configuration.BitStreamVersion;
    }

    public void HandlePacket(IClient client, JoinedGamePacket packet)
    {
        client.SendPacket(new ModNamePacket(this.bitStreamVersion, "deathmatch"));
    }
}
