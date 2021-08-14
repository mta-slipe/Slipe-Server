using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Player
{
    public class PlayerDiagnosticPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_DIAGNOSTIC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        private const int levelSpecialInfo = 236;

        public uint Level { get; set; }
        public string Message { get; set; }

        public IEnumerable<byte> DetectedAC { get; set; }
        public uint D3d9Size { get; set; }
        public string D3d9Md5 { get; set; }
        public string D3d9Sha256 { get; set; }
        public PlayerDiagnosticPacket(uint level, string message, IEnumerable<byte> detectedAC, uint d3d9Size, string d3d9Md5, string d3d9Sha256)
        {
            this.Level = level;
            this.Message = message;
            this.DetectedAC = detectedAC;
            this.D3d9Size = d3d9Size;
            this.D3d9Md5 = d3d9Md5;
            this.D3d9Sha256 = d3d9Sha256;
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
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
                if(parts.Length == 4)
                {
                    DetectedAC = parts[0].Split("|").Select(e => byte.Parse(e));
                    D3d9Size = uint.Parse(parts[1]);
                    D3d9Md5 = parts[2];
                    D3d9Sha256 = parts[3];
                }
            }
            else
            {
                Message = splitMessage[1];
            }
        }
    }
}
