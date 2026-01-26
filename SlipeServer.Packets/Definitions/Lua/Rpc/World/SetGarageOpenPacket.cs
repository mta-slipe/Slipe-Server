using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class SetGarageOpenPacket(byte garageID, bool isOpen) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte GarageID { get; set; } = garageID;
    public bool IsOpen { get; set; } = isOpen;

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_GARAGE_OPEN);
        builder.Write(this.GarageID);
        builder.Write(this.IsOpen);

        return builder.Build();
    }
}
