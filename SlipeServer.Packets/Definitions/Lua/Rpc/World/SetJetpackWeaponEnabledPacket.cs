using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class SetJetpackWeaponEnabledPacket(byte weaponType, bool enabled) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte WeaponType { get; set; } = weaponType;
    public bool Enabled { get; set; } = enabled;

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_JETPACK_WEAPON_ENABLED);
        builder.Write(this.WeaponType);
        builder.Write(this.Enabled);

        return builder.Build();
    }
}
