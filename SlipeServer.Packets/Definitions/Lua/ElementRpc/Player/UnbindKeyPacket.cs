using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;

public class UnbindKeyPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public string Key { get; set; }
    public bool HitState { get; set; }

    public UnbindKeyPacket(uint elementId, string key, bool hitState)
    {
        this.ElementId = elementId;
        this.Key = key;
        this.HitState = hitState;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.UNBIND_KEY);

        builder.WriteStringWithByteAsLength(this.Key);
        builder.Write((byte)(this.HitState ? 1 : 0));

        return builder.Build();
    }

    public override void Reset()
    {

    }
}
