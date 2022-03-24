using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetJetpackMaxHeightPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public float MaxHeight { get; }

        public SetJetpackMaxHeightPacket(float maxHeight)
        {
            this.MaxHeight = maxHeight;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_JETPACK_MAXHEIGHT);
            builder.Write(this.MaxHeight);

            return builder.Build();
        }
    }
}
