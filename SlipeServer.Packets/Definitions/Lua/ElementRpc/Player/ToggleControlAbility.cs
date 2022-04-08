using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;

public class ToggleControlAbility : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public string Control { get; set; }
    public bool Enabled { get; set; }

    public ToggleControlAbility(string control, bool enabled)
    {
        this.Control = control;
        this.Enabled = enabled;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.TOGGLE_CONTROL_ABILITY);
        builder.WriteStringWithByteAsLength(this.Control);
        builder.Write((byte)(this.Enabled ? 1 : 0));
        return builder.Build();
    }
}
