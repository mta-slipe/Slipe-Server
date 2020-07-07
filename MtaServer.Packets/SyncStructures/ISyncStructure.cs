using MtaServer.Packets.Builder;
using MtaServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Structures
{
    public interface ISyncStructure
    {
        void Read(PacketReader reader);
        void Write(PacketBuilder builder);
    }
}
