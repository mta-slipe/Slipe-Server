using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Structures;

public interface ISyncStructure
{
    void Read(PacketReader reader);
    void Write(PacketBuilder builder);
}
