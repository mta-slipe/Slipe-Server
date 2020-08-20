using MtaServer.Packets.Builder;
using MtaServer.Packets.Definitions.Lua.ElementRpc;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetCloudsEnabledPacket : Packet

    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public bool CloudsEnabled { get; set; }

        public SetCloudsEnabledPacket(bool cloudsEnabled)
        {
            this.CloudsEnabled = cloudsEnabled;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_CLOUDS_ENABLED);
            builder.Write(this.CloudsEnabled);

            return builder.Build();
        }
    }
}
