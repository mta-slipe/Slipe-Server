using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Packets.Structures;

public interface ISyncStructure
{
    void Read(PacketReader reader);
    void Write(PacketBuilder builder);
}
