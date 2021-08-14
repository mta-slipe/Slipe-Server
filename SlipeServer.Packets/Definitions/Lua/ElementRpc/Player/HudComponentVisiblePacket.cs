using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player
{
    public class HudComponentVisiblePacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public byte HudComponent { get; set; }
        public bool Show { get; set; }

        public HudComponentVisiblePacket(byte hudComponent, bool show)
        {
            this.HudComponent = hudComponent;
            this.Show = show;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SHOW_PLAYER_HUD_COMPONENT);
            builder.Write((byte)this.HudComponent);
            builder.Write((byte)(this.Show ? 1 : 0));
            return builder.Build();
        }
    }
}
