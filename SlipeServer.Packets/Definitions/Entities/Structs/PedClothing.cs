namespace SlipeServer.Packets.Definitions.Entities.Structs;

public struct PedClothing(string texture, string model, byte type)
{
    public string Texture { get; set; } = texture;
    public string Model { get; set; } = model;
    public byte Type { get; set; } = type;
}
