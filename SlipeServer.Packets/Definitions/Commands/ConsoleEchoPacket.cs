using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlipeServer.Packets.Definitions.Commands
{
    public class ConsoleEchoPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_CONSOLE_ECHO;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.Low;

        public string Message { get; set; } = string.Empty;

        public ConsoleEchoPacket()
        {

        }

        public ConsoleEchoPacket(string message)
        {
            this.Message = message;
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
