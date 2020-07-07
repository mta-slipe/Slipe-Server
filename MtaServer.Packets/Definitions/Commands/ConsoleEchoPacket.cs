using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MtaServer.Packets.Definitions.Commands
{
    public class ConsoleEchoPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_CONSOLE_ECHO;
        public override PacketFlags Flags => PacketFlags.PACKET_LOW_PRIORITY;

        public string Message { get; set; }

        public ConsoleEchoPacket()
        {

        }

        public ConsoleEchoPacket(string message)
        {
            Message = message;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteStringWithByteAsLength(this.Message);

            return builder.Build();
        }
    }
}
