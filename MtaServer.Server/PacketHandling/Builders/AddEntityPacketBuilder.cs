using MtaServer.Packets.Definitions.Entities.Structs;
using MtaServer.Packets.Definitions.Lua.ElementRpc.Element;
using MtaServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Server.PacketHandling.Builders
{
    public class AddEntityPacketBuilder
    {
        private readonly AddEntityPacket packet;

        public AddEntityPacketBuilder()
        {
            this.packet = new AddEntityPacket();
        }

        public void AddDummy(DummyElement element)
        {
            packet.AddDummy(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext, 
                "resource", element.Position);
        }

        public AddEntityPacket Build()
        {
            return this.packet;
        }
    }
}
