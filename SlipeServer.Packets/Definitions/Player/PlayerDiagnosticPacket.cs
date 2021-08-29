using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Packets.Definitions.Player
{
    public class PlayerDiagnosticPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_DIAGNOSTIC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public const int levelSpecialInfo = 236;

        public uint Level { get; set; }
        public string Message { get; set; } = string.Empty;

        public IEnumerable<byte> DetectedAC { get; set; } = Array.Empty<byte>();
        public uint D3d9Size { get; set; }
        public string D3d9Md5 { get; set; } = string.Empty;
        public string D3d9Sha256 { get; set; } = string.Empty;
        public PlayerDiagnosticPacket()
        {
        }

        public override byte[] Write()
        {
            throw new NotSupportedException();
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            string message = reader.GetString();
            var splitMessage = message.Split(",", 2);
            this.Level = uint.Parse(splitMessage[0]);

            if (this.Level == levelSpecialInfo)
            {
                var parts = splitMessage[1].Split(",");
                if (parts.Length == 4)
                {
                    this.DetectedAC = parts[0].Split("|").Select(e => byte.Parse(e));
                    this.D3d9Size = uint.Parse(parts[1]);
                    this.D3d9Md5 = parts[2];
                    this.D3d9Sha256 = parts[3];
                }
            } else
            {
                this.Message = splitMessage[1];
            }
        }

        public override void Reset()
        {
            this.DetectedAC = Array.Empty<byte>();
            this.D3d9Size = 0;
            this.D3d9Md5 = string.Empty;
            this.D3d9Sha256 = string.Empty;
            this.Message = string.Empty;
        }
    }
}
