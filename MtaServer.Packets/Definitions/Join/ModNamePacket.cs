using MtaServer.Packets;
using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;

namespace MTAServerWrapper.Packets.Outgoing.Connection
{
    public class ModNamePacket: Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_MOD_NAME;
        public override PacketFlags Flags => throw new NotImplementedException();

        public ushort NetVersion { get; }
        public string Name { get; }

        public ModNamePacket(ushort netVersion, string name)
        {
            NetVersion = netVersion;
            Name = name;
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write(this.NetVersion);
            builder.Write(this.Name);

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
