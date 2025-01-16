using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Ped;

public sealed class PedWastedPacket : Packet
{
    public override PacketId PacketId { get; } = PacketId.PACKET_ID_PED_WASTED;
    public override PacketReliability Reliability { get; } = PacketReliability.ReliableSequenced;
    public override PacketPriority Priority { get; } = PacketPriority.High;

    public ElementId SourceElementId { get; set; }
    public ElementId KillerId { get; set; }
    public byte KillerWeapon { get; set; }
    public byte BodyPart { get; set; }
    public Vector3 Position { get; set; }
    public ushort Ammo { get; set; }
    public bool Stealth { get; set; }
    public byte TimeContext { get; set; }
    public ulong AnimGroup { get; set; }
    public ulong AnimId { get; set; }

    public PedWastedPacket(ElementId sourceElementId, ElementId killerId, byte killerWeapon, byte bodyPart, ushort ammo, bool stealth, byte timeContext, ulong animGroup, ulong animId)
    {
        this.SourceElementId = sourceElementId;
        this.KillerId = killerId;
        this.KillerWeapon = killerWeapon;
        this.BodyPart = bodyPart;
        this.Ammo = ammo;
        this.Stealth = stealth;
        this.TimeContext = timeContext;
        this.AnimGroup = animGroup;
        this.AnimId = animId;
    }

    public PedWastedPacket()
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.SourceElementId);
        builder.Write(this.KillerId);

        builder.WriteWeaponType(this.KillerWeapon);
        builder.WriteBodyPart(this.BodyPart);

        builder.Write(this.Stealth);
        builder.Write(this.TimeContext);

        builder.WriteCompressed(this.AnimGroup);
        builder.WriteCompressed(this.AnimId);

        return builder.Build();
    }

    public override void Read(byte[] bytes)
    {
        var data = new PacketReader(bytes);

        this.AnimGroup = data.GetCompressedUInt32();
        this.AnimId = data.GetCompressedUInt32();
        this.KillerId = data.GetElementId();
        this.KillerWeapon = data.GetWeaponType();
        this.BodyPart = data.GetBodyPart();
        this.Position = data.GetVector3WithZAsFloat();
        this.SourceElementId = data.GetElementId();

        this.Ammo = data.GetAmmo();
    }
}
