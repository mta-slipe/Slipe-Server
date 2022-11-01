using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Weapons;
public class SetWeaponPropertyRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public byte Weapon { get; }
    public byte Property { get; }
    public byte SkillLevel { get; }
    public bool? IsEnabled { get; }
    public float? FloatValue { get; }
    public short? ShortValue { get; }

    public SetWeaponPropertyRpcPacket(byte weapon, byte property, byte skillLevel, bool isEnabled)
    {
        this.Weapon = weapon;
        this.Property = property;
        this.SkillLevel = skillLevel;
        this.IsEnabled = isEnabled;
    }

    public SetWeaponPropertyRpcPacket(byte weapon, byte property, byte skillLevel, float value)
    {
        this.Weapon = weapon;
        this.Property = property;
        this.SkillLevel = skillLevel;
        this.FloatValue = value;
    }

    public SetWeaponPropertyRpcPacket(byte weapon, byte property, byte skillLevel, short value)
    {
        this.Weapon = weapon;
        this.Property = property;
        this.SkillLevel = skillLevel;
        this.ShortValue = value;
    }

    public SetWeaponPropertyRpcPacket(byte weapon, byte property, byte skillLevel, int value)
        : this(weapon, property, skillLevel, (short)value) { }

    public SetWeaponPropertyRpcPacket(byte weapon, byte property, byte skillLevel, ulong value)
        : this(weapon, property, skillLevel, (short)value) { }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_WEAPON_PROPERTY);
        builder.Write(this.Weapon);
        builder.Write(this.Property);
        builder.Write(this.SkillLevel);

        if (this.IsEnabled.HasValue)
            builder.Write(this.IsEnabled.Value);

        if (this.FloatValue.HasValue)
            builder.Write(this.FloatValue.Value);

        if (this.ShortValue.HasValue)
            builder.Write(this.ShortValue.Value);

        return builder.Build();
    }
}
