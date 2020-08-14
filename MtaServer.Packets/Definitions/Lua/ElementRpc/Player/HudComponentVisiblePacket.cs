using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;

namespace MtaServer.Packets.Definitions.Lua.ElementRpc.Player
{
    public enum HudComponent
    {
        Ammo,
        Weapon,
        Health,
        Breath,
        Armour,
        Money,
        VehicleName,
        AreaName,
        Radar,
        Clock,
        Radio,
        Wanted,
        Crosshair,
        All,
    };

    public class HudComponentVisiblePacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketFlags Flags => PacketFlags.PACKET_HIGH_PRIORITY | PacketFlags.PACKET_RELIABLE | PacketFlags.PACKET_SEQUENCED;

        public HudComponent HudComponent { get; set; }
        public bool Show { get; set; }

        public HudComponentVisiblePacket(HudComponent hudComponent, bool show)
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
            builder.Write((byte)HudComponent);
            builder.Write((byte)(Show ? 1 : 0));
            return builder.Build();
        }
    }
}
