using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.ElementRpc.Player
{
    public class ToggleAllControlsPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketFlags Flags => PacketFlags.PACKET_HIGH_PRIORITY | PacketFlags.PACKET_RELIABLE | PacketFlags.PACKET_SEQUENCED;

        public bool GTAControls { get; set; }
        public bool MTAControls { get; set; }
        public bool Enabled { get; set; }

        public ToggleAllControlsPacket(bool enabled, bool gtaControls, bool mtaControls)
        {
            this.Enabled = enabled;
            this.GTAControls = gtaControls;
            this.MTAControls = mtaControls;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.TOGGLE_ALL_CONTROL_ABILITY);
            builder.Write((byte)(GTAControls ? 1 : 0));
            builder.Write((byte)(MTAControls ? 1 : 0));
            builder.Write((byte)(Enabled ? 1 : 0));
            return builder.Build();
        }
    }
}
