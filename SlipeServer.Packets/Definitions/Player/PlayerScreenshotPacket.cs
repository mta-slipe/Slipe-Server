using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Player
{
    public class PlayerScreenshotPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_SCREENSHOT;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public ScreenshotStatus Status { get; set; }
        public ushort ScreenshotId { get; set; }
        public ushort PartNumber { get; set; }
        public long ServerGrabTime { get; set; }
        public uint TotalBytes { get; set; }
        public ushort TotalParts { get; set; }

        public ushort ResourceId { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;

        public byte[] Buffer { get; set; } = Array.Empty<byte>();

        public PlayerScreenshotPacket()
        {
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.Status = (ScreenshotStatus)reader.GetByte();

            switch (this.Status)
            {
                case ScreenshotStatus.Success:
                    this.ScreenshotId = reader.GetUint16();
                    this.PartNumber = reader.GetUint16();
                    ushort partBytes = reader.GetUint16();
                    this.Buffer = reader.GetBytes(partBytes);
                    if(this.PartNumber == 0)
                    {
                        this.ServerGrabTime = reader.GetUint32();
                        this.TotalBytes = reader.GetUint32();
                        this.TotalParts = reader.GetUint16();
                        this.ResourceId = reader.GetUint16();
                        this.Tag = reader.GetString();
                    }
                    break;
                case ScreenshotStatus.Unknown:
                    break;
                case ScreenshotStatus.Minimalized:
                case ScreenshotStatus.Disabled:
                case ScreenshotStatus.Error:
                    this.ServerGrabTime = reader.GetUint32();
                    this.ResourceId = reader.GetUint16();
                    this.Tag = reader.GetString();
                    if (!reader.IsFinishedReading)
                        this.Error = reader.GetString();
                    else
                        this.Error = this.Status.ToString();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override byte[] Write()
        {
            throw new NotSupportedException();
        }
    }
}
