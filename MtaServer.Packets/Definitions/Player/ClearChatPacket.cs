using MtaServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;

namespace MtaServer.Packets.Definitions.Player
{
    public class ClearChatPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_CHAT_CLEAR;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public ClearChatPacket()
        {

        }

        public override void Read(byte[] bytes)
        {
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write(false);
            return builder.Build();
        }
    }
}
