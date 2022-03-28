using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.Weapons;

public class SetWeaponPropertyPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte Weapon { get; set; }
    public byte Property { get; set; }
    public byte SkillLevel { get; set; }
    public int Data { get; set; }

    public SetWeaponPropertyPacket(byte weapon, byte property, byte skillLevel, int data)
    {
        this.Weapon = weapon;
        this.Property = property;
        this.SkillLevel = skillLevel;
        this.Data = data;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_WEAPON_PROPERTY);
        builder.Write(this.Weapon);
        builder.Write(this.Property);
        builder.Write(this.SkillLevel);
        builder.Write(this.Weapon);

        return builder.Build();
    }
}
