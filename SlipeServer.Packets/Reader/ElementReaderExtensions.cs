namespace SlipeServer.Packets.Reader;

public static class ElementReaderExtensions
{
    public static float GetPlayerHealth(this PacketReader reader)
        => reader.GetFloatFromBits(8, 0, 255);
    public static float GetPlayerArmor(this PacketReader reader)
        => reader.GetFloatFromBits(8, 0, 127.5f);
}
