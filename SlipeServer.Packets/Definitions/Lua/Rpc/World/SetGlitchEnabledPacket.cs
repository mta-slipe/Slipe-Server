using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetGlitchEnabledPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public byte GlitchType { get; set; }
        public byte Enabled { get; set; }

        public SetGlitchEnabledPacket(byte glitchType, bool enabled)
        {
            this.GlitchType = glitchType;
            this.Enabled = enabled ? 1 : 0;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_GLITCH_ENABLED);
            builder.Write(this.GlitchType);
            builder.Write(this.Enabled);

            return builder.Build();
        }
    }
}
