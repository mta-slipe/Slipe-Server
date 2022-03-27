using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;

namespace SlipeServer.Packets.Lua.Event;

public enum LatentEventFlag : byte
{
    Head,
    Tail,
    Cancel
}

public enum LatentEventCategory : ushort
{
    None,
    Packet
}

public struct LatentEventHeader
{
    public LatentEventCategory Category { get; set; }
    public uint FinalSize { get; set; }
    public uint Rate { get; set; }
    public ushort ResourceNetId { get; set; }
}

public class LatentLuaEventPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LATENT_TRANSFER;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ushort Id { get; set; }
    public LatentEventFlag? Flag { get; set; }
    public LatentEventHeader? Header { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();

    public LatentLuaEventPacket()
    {

    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();

        throw new NotImplementedException();

        return builder.Build();
    }

    public override void Read(byte[] bytes)
    {
        PacketReader reader = new PacketReader(bytes);

        this.Id = BitConverter.ToUInt16(reader.GetBytesCapped(15));

        if (reader.GetBit())
        {
            this.Flag = (LatentEventFlag)reader.GetByte();
            if (this.Flag == LatentEventFlag.Head)
                this.Header = new LatentEventHeader()
                {
                    Category = (LatentEventCategory)reader.GetUint16(),
                    FinalSize = reader.GetUint32(),
                    Rate = reader.GetUint32(),
                    ResourceNetId = reader.GetUint16()
                };
        }

        reader.AlignToByteBoundary();
        var size = reader.GetUint16();
        this.Data = reader.GetBytes(size);
    }
}
