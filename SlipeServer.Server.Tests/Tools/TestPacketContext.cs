using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.TestTools;
using System.Collections.Generic;

namespace SlipeServer.Server.Tests.Tools;

public class TestPacketContext(LightTestNetWrapper netWrapper)
{
    public IEnumerable<SendLuaEvent> GetSentLuaEvents() => netWrapper.GetSentLuaEvents();

    public void VerifyPacketSent(PacketId packetId, Player to, byte[]? data = null, int count = 1)
        => netWrapper.VerifyPacketSent(packetId, to, data, count);

    public void VerifyLuaElementRpcPacketSent(ElementRpcFunction packetId, Player to, byte[]? data = null, int count = 1)
        => netWrapper.VerifyLuaElementRpcPacketSent(packetId, to, data, count);

    public void VerifyLuaEventTriggered(string eventName, Player to, Element source, LuaValue[] luaValues, int expectedCount = 1)
        => netWrapper.VerifyLuaEventTriggered(eventName, to, source, luaValues, expectedCount);

    public void ResetPacketCountVerification() => netWrapper.ResetPacketCountVerification();

    public void SimulatePacketReceived(ulong binaryAddress, PacketId packetId, byte[] data, uint? timestamp = null)
        => netWrapper.SimulatePacketReceived(binaryAddress, packetId, data, timestamp);

    public void VerifyNoPacketsSent() => netWrapper.VerifyNoPacketsSent();
}
