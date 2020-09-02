using MtaServer.Packets;
using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;

namespace MTAServerWrapper.Packets.Outgoing.Connection
{
    public class PlayerTimeoutPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_TIMEOUT;
        public override PacketFlags Flags => throw new NotImplementedException();

        public PlayerTimeoutPacket()
        {
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

        public override void Read(byte[] bytes)
        {

        }
    }
}
