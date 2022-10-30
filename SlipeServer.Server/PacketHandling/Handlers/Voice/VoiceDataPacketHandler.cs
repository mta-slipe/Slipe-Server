using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers.Voice;

public class VoiceDataPacketHandler : IPacketHandler<VoiceDataPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_VOICE_DATA;

    public void HandlePacket(IClient client, VoiceDataPacket packet)
    {
        client.Player.VoiceDataStart(packet.Buffer);
    }
}
