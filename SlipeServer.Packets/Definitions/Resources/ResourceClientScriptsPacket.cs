using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System.Collections.Generic;

namespace SlipeServer.Packets.Definitions.Resources
{
    public class ResourceClientScriptsPacket : Packet
    {

        public override PacketId PacketId => PacketId.PACKET_ID_RESOURCE_CLIENT_SCRIPTS;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public ushort NetId { get; }
        public Dictionary<string, byte[]> Files { get; }

        public ResourceClientScriptsPacket(
            ushort netId,
            Dictionary<string, byte[]> files
        )
        {
            this.NetId = netId;
            this.Files = files;
        }

        public override void Read(byte[] bytes)
        {
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write(this.NetId);
            builder.Write((ushort)this.Files.Count);

            foreach (var kvPair in this.Files)
            {
                builder.Write(kvPair.Key);
                builder.Write((uint)kvPair.Value.Length);
                builder.Write(kvPair.Value);
            }


            return builder.Build();
        }
    }
}
