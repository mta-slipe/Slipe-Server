﻿using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;

namespace SlipeServer.Packets.Rpc;

public sealed class RpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public RpcFunctions FunctionId { get; private set; }

    public PacketReader Reader { get; private set; }

    public RpcPacket()
    {
        this.Reader = new PacketReader(Array.Empty<byte>());
    }

    public override void Read(byte[] bytes)
    {
        this.Reader = new PacketReader(bytes);

        this.FunctionId = (RpcFunctions)this.Reader.GetByte();
    }

    public override byte[] Write()
    {
        throw new NotSupportedException();
    }
}
