using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.Camera;

public class SetCameraInteriorPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte Interior { get; set; }

    public SetCameraInteriorPacket(byte interior)
    {
        this.Interior = interior;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_CAMERA_INTERIOR);
        builder.Write(this.Interior);

        return builder.Build();
    }
}
