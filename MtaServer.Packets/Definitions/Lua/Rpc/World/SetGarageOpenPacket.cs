using MtaServer.Packets.Builder;
using MtaServer.Packets.Definitions.Lua.ElementRpc;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetGarageOpenPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public byte GarageID { get; set; }
        public bool IsOpen { get; set; }

        public SetGarageOpenPacket(byte garageID,bool isOpen)
        {
            this.GarageID = garageID;
            this.IsOpen = isOpen;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_GARAGE_OPEN);
            builder.Write(this.GarageID);
            builder.Write(this.IsOpen);

            return builder.Build();
        }
    }
}
