using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player
{
    public class ToggleAllControlsPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

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
