using MtaServer.Packets.Enums;
using MtaServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtaServer.Packets.Rpc
{
    public class RpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_RPC;
        public override PacketFlags Flags => throw new NotImplementedException();

        public RpcFunctions FunctionId { get; private set; }

        public override void Read(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);

            this.FunctionId = (RpcFunctions)reader.GetByte();
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }
    }
}
