using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public override PacketPriority Priority => PacketPriority.Low;

    public ushort Id { get; set; }
    public LatentEventFlag? Flag { get; set; }
    public LatentEventHeader? Header { get; set; }
    public IEnumerable<byte> Data { get; set; } = Array.Empty<byte>();

    public LatentLuaEventPacket()
    {

    }

    public override void Reset()
    {
        this.Header = null;
        this.Flag = null;
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();

        builder.WriteBytesCapped(BitConverter.GetBytes(this.Id), 15);
        builder.Write(this.Flag != null);
        if (this.Flag != null)
        {
            builder.Write((byte)this.Flag);

            if (this.Flag == LatentEventFlag.Head)
            {
                if (this.Header == null)
                    throw new ArgumentNullException("Can not send a latent packet with Flag Head without header");

                builder.Write((ushort)this.Header.Value.Category);
                builder.Write(this.Header.Value.FinalSize);
                builder.Write(this.Header.Value.Rate);
                builder.Write(this.Header.Value.ResourceNetId);
            }
        }

        builder.AlignToByteBoundary();
        builder.Write((ushort)this.Data.Count());
        builder.Write(this.Data);

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
