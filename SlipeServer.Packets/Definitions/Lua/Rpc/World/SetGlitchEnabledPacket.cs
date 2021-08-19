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

        public GlitchType GlitchType { get; set; }
        public bool Enabled { get; set; }

        public SetGlitchEnabledPacket(GlitchType glitchType, bool enabled)
        {
            this.GlitchType = glitchType;
            this.Enabled = enabled;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_GLITCH_ENABLED);
            builder.Write(this.Enabled);

            return builder.Build();
        }
    }
}
