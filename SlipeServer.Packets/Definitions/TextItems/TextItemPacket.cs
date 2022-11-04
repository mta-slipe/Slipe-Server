using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Packets.DefinitionsTextITems;

public class TextItemPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_TEXT_ITEM;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ulong Id { get; set; }
    public bool IsDelete { get; set; }
    public Vector2 Position { get; set; }
    public float Scale { get; set; }
    public Color Color { get; set; }
    public byte Format { get; set; }
    public byte ShadowAlpha { get; set; }
    public string Text { get; set; } = string.Empty;

    public TextItemPacket(ulong id)
    {
        this.Id = id;
        this.IsDelete = true;
    }

    public TextItemPacket(
        ulong id,
        Vector2 position,
        string text,
        float scale = 1,
        Color? color = null,
        byte format = 0,
        byte shadowAlpha = 255)
    {
        this.Id = id;
        this.Position = position;
        this.Text = text;
        this.Scale = scale;
        this.Color = color ?? Color.White;
        this.Format = format;
        this.ShadowAlpha = shadowAlpha;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.WriteCompressed(this.Id);
        builder.Write(this.IsDelete);

        if (!this.IsDelete)
        {
            builder.Write(this.Position);
            builder.Write(this.Scale);
            builder.Write(this.Color, true);
            builder.Write(this.Format);
            builder.Write(this.ShadowAlpha);
            builder.WriteCompressed((ushort)(this.Text.Length));
            builder.WriteStringWithoutLength(this.Text);
        }

        return builder.Build();
    }
}
