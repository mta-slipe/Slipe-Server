using SlipeServer.Packets.Definitions.Lua;

namespace SlipeServer.Server.Mappers;
public interface ILuaMappable
{
    LuaValue ToLuaValue();
}
