using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlipeServer.Packets.Definitions.Commands
{
    public class ChatEchoPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_CHAT_ECHO;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.Low;

        public uint SourceId { get; set; }
        public string Message { get; set; } = string.Empty;
        public Color Color { get; set; }
        public byte MessageType { get; set; }
        public bool IsColorCoded { get; set; }

        public ChatEchoPacket()
        {

        }

        public ChatEchoPacket(uint sourceId, string message, Color color, ChatEchoType messageType, bool isColorCoded = false)
        {
            this.SourceId = sourceId;
            this.Message = message;
            this.Color = color;
            this.MessageType = (byte)messageType;
            this.IsColorCoded = isColorCoded;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write(this.Color);
            builder.Write(this.IsColorCoded);
            builder.WriteElementId(this.SourceId);
            //builder.Write(this.MessageType); // to be added when upping bitstream version to 116
            builder.WriteStringWithoutLength(this.Message);

            return builder.Build();
        }
    }
}
