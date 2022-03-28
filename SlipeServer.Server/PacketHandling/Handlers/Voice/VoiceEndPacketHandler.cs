using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers.Voice;

public class VoiceEndPacketHandler : IPacketHandler<VoiceEndPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_VOICE_END;

    public void HandlePacket(Client client, VoiceEndPacket packet)
    {
        client.Player.VoiceDataEnd();
    }
}
