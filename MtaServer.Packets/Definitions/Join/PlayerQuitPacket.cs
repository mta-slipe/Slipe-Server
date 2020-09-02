using MtaServer.Packets;
using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;

namespace MTAServerWrapper.Packets.Outgoing.Connection
{
    public class PlayerQuitPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_QUIT;
        public override PacketFlags Flags => throw new NotImplementedException();

        public uint PlayerId { get; }
        public byte QuitReason { get; }

        public PlayerQuitPacket(uint playerId, byte quitReason)
        {
            PlayerId = playerId;
            QuitReason = quitReason;
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(PlayerId);
            builder.WriteCapped(QuitReason, 3);

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {

        }
    }
}
