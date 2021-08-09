using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Player
{
    public enum ScreenshotStatus : byte
    {
        Unknown,
        Success,
        Minimalized,
        Disabled,
        Error,
    }

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
        public string Tag { get; set; }
        public string Error { get; set; }

        public byte[] Buffer { get; set; }
        //CBuffer m_buffer;

        public PlayerScreenshotPacket(
        )
        {
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            Status = (ScreenshotStatus)reader.GetByte();

            switch (Status)
            {
                case ScreenshotStatus.Success:
                    ScreenshotId = reader.GetUint16();
                    PartNumber = reader.GetUint16();
                    ushort partBytes = reader.GetUint16();
                    Buffer = reader.GetBytes(partBytes);
                    if(PartNumber == 0)
                    {
                        ServerGrabTime = reader.GetUint32();
                        TotalBytes = reader.GetUint32();
                        TotalParts = reader.GetUint16();
                    }
                    break;
                case ScreenshotStatus.Unknown:
                    break;
                case ScreenshotStatus.Minimalized:
                case ScreenshotStatus.Disabled:
                case ScreenshotStatus.Error:

                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }
    }
}
