using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player
{
    public class SetTransferBoxVisiblePacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public bool Visible { get; set; }

        public SetTransferBoxVisiblePacket(bool visible)
        {
            this.Visible = visible;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_TRANSFER_BOX_VISIBILITY);
            builder.Write(this.Visible);
            return builder.Build();
        }
    }
}
