namespace SlipeServer.Packets.Builder;

public static class PedBuilderExtensions
{
    public static void WriteBodyPart(this PacketBuilder builder, byte bodypart) => builder.WriteCapped(bodypart, 3);
}
