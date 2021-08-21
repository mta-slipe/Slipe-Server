using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;

namespace SlipeServer.Packets.Definitions.Player
{
    public class PlayerNetworkStatusPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_NETWORK_STATUS;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public PlayerNetworkStatusType Type { get; set; }
        public uint Ticks { get; set; }

        public PlayerNetworkStatusPacket()
        {
        }

        public override void Read(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            switch(reader.GetByte())
            {
                case 0:
                    this.Type = PlayerNetworkStatusType.InterruptionBegan;
                    break;
                case 1:
                    this.Type = PlayerNetworkStatusType.InterruptionEnd;
                    break;
                default:
                    throw new NotSupportedException();
            }
            this.Ticks = reader.GetUint32();
        }

        public override byte[] Write()
        {
            throw new NotSupportedException();
        }
    }
}
