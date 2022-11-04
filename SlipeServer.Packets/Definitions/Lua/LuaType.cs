namespace SlipeServer.Packets.Definitions.Lua;

public enum LuaType
{
    None = -1,
    Nil,
    Boolean,
    LightUserdata,
    Number,
    String,
    Table,
    Function,
    Userdata,
    Thread,
    TableRef,
    LongString
}
