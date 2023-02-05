using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.CustomElementData;

public class CustomDataPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_CUSTOM_DATA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public LuaValue Value { get; set; }
    public string Key { get; set; }

    private const ushort maxDataNameLength = 128;

    public CustomDataPacket()
    {
        this.Value = new LuaValue();
        this.Key = "";
    }

    public CustomDataPacket(ElementId elementId, string name, LuaValue value)
    {
        this.ElementId = elementId;
        this.Key = name;
        this.Value = value;
    }

    public override byte[] Write() => throw new NotImplementedException();

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        this.ElementId = reader.GetElementId();
        ushort nameLength = reader.GetCompressedUint16();

        if (nameLength > 0 && nameLength <= maxDataNameLength)
        {
            this.Key = reader.GetStringCharacters(nameLength);
            this.Value = reader.GetLuaValue();
        }
    }
}

