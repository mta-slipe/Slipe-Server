using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class SetVehicleRotationRpcPacket : Packet
    { 
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketFlags Flags => PacketFlags.PACKET_HIGH_PRIORITY | PacketFlags.PACKET_RELIABLE | PacketFlags.PACKET_SEQUENCED;

        public uint ElementId { get; set; }
        public byte TimeContext { get; set; }
        public Vector3 Rotation { get; set; }

        public SetVehicleRotationRpcPacket()
        {

        }

        public SetVehicleRotationRpcPacket(uint elementId, byte timeContext, Vector3 rotation)
        {
            this.ElementId = elementId;
            this.TimeContext = timeContext;
            this.Rotation = rotation;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write((byte)ElementRpcFunction.SET_VEHICLE_ROTATION);
            builder.WriteElementId(this.ElementId);

            builder.Write(this.Rotation);

            builder.Write(this.TimeContext);

            return builder.Build();
        }
    }
}
