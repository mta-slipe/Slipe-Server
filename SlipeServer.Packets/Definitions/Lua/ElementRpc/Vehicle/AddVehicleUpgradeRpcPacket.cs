using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle
{
    public class AddVehicleUpgradeRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public ushort UpgradeId { get; set; }

        public AddVehicleUpgradeRpcPacket(uint elementId, ushort upgradeId)
        {
            this.ElementId = elementId;
            this.UpgradeId = upgradeId;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.ADD_VEHICLE_UPGRADE);
            builder.WriteElementId(this.ElementId);
            builder.Write(this.UpgradeId);
            return builder.Build();
        }
    }
}
