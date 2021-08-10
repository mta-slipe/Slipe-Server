using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Player
{
    public class PlayerDiagnosticPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_DIAGNOSTIC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public string Message { get; set; }

        public PlayerDiagnosticPacket()
        {

        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.Message = reader.GetString();
        }
    }
}
