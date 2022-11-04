namespace SlipeServer.Packets.Builder;

public static class ElementBuilderExtensions
{
    public static void WritePlayerHealth(this PacketBuilder builder, float health)
        => builder.WriteFloatFromBits(health, 8, 0, 255, true, false);

    public static void WritePlayerArmor(this PacketBuilder builder, float armor)
        => builder.WriteFloatFromBits(armor, 8, 0, 127.5f, true, false);
}
