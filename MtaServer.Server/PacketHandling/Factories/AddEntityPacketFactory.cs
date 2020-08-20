using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Definitions.Lua.ElementRpc.Element;
using MtaServer.Server.Elements;
using MtaServer.Server.PacketHandling.Builders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace MtaServer.Server.PacketHandling.Factories
{
    public static class AddEntityPacketFactory
    {
        public static AddEntityPacket CreateAddEntityPacket(Element[] elements)
        {
            var builder = new AddEntityPacketBuilder();

            foreach(var element in elements)
            {
                switch (element.ElementType)
                {
                    case ElementType.Dummy:
                        if (element is DummyElement dummy)
                            builder.AddDummy(dummy);
                        break;
                }
            }

            return builder.Build();
        }
    }
}
