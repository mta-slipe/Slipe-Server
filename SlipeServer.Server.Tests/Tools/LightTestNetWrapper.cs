using FluentAssertions;
using SlipeServer.Net.Wrappers;
using SlipeServer.Net.Wrappers.Enums;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Elements;
using SlipeServer.Server.TestTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SlipeServer.Server.Tests.Tools;

public class LightTestNetWrapper : INetWrapper
{
    private readonly ConcurrentStack<SendPacketCall> sentPackets = [];

    public Tuple<string, string, string> GetClientSerialExtraAndVersion(ulong binaryAddress) => new("TEST-SERIAL", "TEST-EXTRA", "TEST-VERSION");
    public IPAddress GetPlayerIp(ulong binaryAddress) => IPAddress.Any;
    public void ResendModPackets(ulong binaryAddress) { }
    public void ResendPlayerACInfo(ulong binaryAddress) {}
    public void SetAntiCheatConfig(IEnumerable<AntiCheat> disabledAntiCheats, bool hideAntiCheatFromClient, AllowGta3ImgMods allowGta3ImgMods, IEnumerable<SpecialDetection> enabledSpecialDetections, DataFile disallowedDataFiles) { }
    public void SetVersion(ulong binaryAddress, ushort version) { }
    public void Start(){ }
    public void Stop() { }

    public void SendPacket(ulong binaryAddress, PacketId packetId, ushort bitStreamVersion, byte[] data, PacketPriority priority = PacketPriority.High, PacketReliability reliability = PacketReliability.ReliableSequenced)
    {
        this.sentPackets.Push(new SendPacketCall
        {
            Address = binaryAddress,
            Version = bitStreamVersion,
            PacketId = packetId,
            Data = data,
            Priority = priority,
            Reliability = reliability
        });
    }

    public void SendPacket(ulong binaryAddress, ushort bitStreamVersion, Packet packet)
    {
        this.sentPackets.Push(new SendPacketCall
        {
            Address = binaryAddress,
            Version = bitStreamVersion,
            PacketId = packet.PacketId,
            Data = packet.Write(),
            Priority = packet.Priority,
            Reliability = packet.Reliability
        });
    }

    public IEnumerable<SendLuaEvent> GetSentLuaEvents()
    {
        foreach (var sendPacketCall in this.sentPackets)
        {
            if (sendPacketCall.PacketId == PacketId.PACKET_ID_LUA_EVENT)
            {
                var luaEventPacket = new LuaEventPacket();
                luaEventPacket.Read(sendPacketCall.Data);
                yield return new SendLuaEvent
                {
                    Address = sendPacketCall.Address,
                    Name = luaEventPacket.Name,
                    Arguments = luaEventPacket.LuaValues.ToArray(),
                    Source = luaEventPacket.ElementId
                };
            }
        }
    }

    public void VerifyPacketSent(PacketId packetId, Player to, byte[]? data = null, int count = 1)
    {
        this.sentPackets.Count(x =>
            x.PacketId == packetId && x.Address == to.GetAddress() && (data == null || x.Data.SequenceEqual(data))
        ).Should().Be(count);
    }

    public void VerifyLuaElementRpcPacketSent(ElementRpcFunction packetId, Player to, byte[]? data = null, int count = 1)
    {
        this.sentPackets.Count(x =>
            x.PacketId == PacketId.PACKET_ID_LUA_ELEMENT_RPC &&
            x.Address == to.GetAddress() &&
            x.Data[0] == (byte)packetId
            && (data == null || x.Data.SequenceEqual(data))
        ).Should().Be(count);
    }

    public void VerifyLuaEventTriggered(string eventName, Player to, Element source, LuaValue[] luaValues, int expectedCount = 1)
    {
        var luaEventPacket = new LuaEventPacket();
        int count = 0;
        foreach (var sendPacketCall in this.sentPackets)
        {
            if (sendPacketCall.PacketId == PacketId.PACKET_ID_LUA_EVENT)
            {
                luaEventPacket.Read(sendPacketCall.Data);
                if (luaEventPacket.Name == eventName && luaEventPacket.ElementId == source.Id && sendPacketCall.Address == to.GetAddress())
                {
                    var packetLuaValues = luaEventPacket.LuaValues.ToArray();
                    if (packetLuaValues.Length != luaValues.Length)
                        break;

                    for (int i = 0; i < luaValues.Length; i++)
                    {
                        if (packetLuaValues[i] != luaValues[i])
                            break;
                    }

                    count++;
                }
            }
        }

        count.Should().Be(expectedCount);
    }

    public void VerifyNoPacketsSent()
    {
        this.sentPackets.Should().BeEmpty();
    }

    public int GetPacketCount(PacketId packetId, Player? forPlayer = null)
    {
        return this.sentPackets.Count(x =>
            x.PacketId == packetId &&
            (forPlayer == null || x.Address == forPlayer.GetAddress())
        );
    }

    public void ResetPacketCountVerification() => this.sentPackets.Clear();

    public void SimulatePacketReceived(ulong binaryAddress, PacketId packetId, byte[] data, uint? timestamp = null)
    {
        this.PacketReceived?.Invoke(this, binaryAddress, packetId, data, timestamp);
    }

    public event Action<INetWrapper, ulong, PacketId, byte[], uint?>? PacketReceived;

    private struct SendPacketCall
    {
        public ulong Address { get; set; }
        public ushort Version { get; set; }

        public PacketId PacketId { get; set; }
        public byte[] Data { get; set; }
        public PacketReliability Reliability { get; set; }
        public PacketPriority Priority { get; set; }
    }

}
