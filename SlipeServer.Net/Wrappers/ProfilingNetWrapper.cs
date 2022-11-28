using SlipeServer.Net.Wrappers.Enums;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SlipeServer.Net.Wrappers;

public class ProfilingNetWrapper : NetWrapper, IDisposable, INetWrapper
{
    private readonly HashSet<PacketId> incomingTaggedPacketIds;
    private readonly HashSet<PacketId> outgoingTaggedPacketIds;

    public ProfilingNetWrapper(string directory, string netDllPath, string host, ushort port) : base(directory, netDllPath, host, port)
    {
        this.incomingTaggedPacketIds = new();
        this.outgoingTaggedPacketIds = new();
    }

    protected override void SendPacket(uint binaryAddress, byte packetId, ushort bitStreamVersion, byte[] payload, PacketPriority priority, PacketReliability reliability)
    {
        base.SendPacket(binaryAddress, packetId, bitStreamVersion, payload, priority, reliability);

        if (this.outgoingTaggedPacketIds.Contains((PacketId)packetId))
            Debugger.Break();
    }

    protected override void PacketInterceptor(byte packetId, uint binaryAddress, IntPtr payload, uint payloadSize, bool hasPing, uint ping)
    {
        base.PacketInterceptor(packetId, binaryAddress, payload, payloadSize, hasPing, ping);

        if (this.incomingTaggedPacketIds.Contains((PacketId)packetId))
            Debugger.Break();
    }

    public void TagIncomingPacket(PacketId packetId) => this.incomingTaggedPacketIds.Add(packetId);
    public void DetagIncomingPacket(PacketId packetId) => this.incomingTaggedPacketIds.Remove(packetId);

    public void TagOutgoingPacket(PacketId packetId) => this.outgoingTaggedPacketIds.Add(packetId);
    public void DetagOutgoingPacket(PacketId packetId) => this.outgoingTaggedPacketIds.Remove(packetId);
}
