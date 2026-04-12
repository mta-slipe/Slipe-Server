using SlipeServer.Packets.Definitions.Lua;
using System.Collections.Generic;

namespace SlipeServer.Scripting.Definitions;

public class AccountHandle
{
    public int Id { get; internal set; }
    public string Name { get; internal set; }
    public bool IsGuest { get; }
    public string? Serial { get; internal set; }
    public string? Ip { get; internal set; }

    internal Dictionary<string, LuaValue> GuestData { get; } = new(System.StringComparer.Ordinal);

    internal AccountHandle(string guestName)
    {
        this.Id = 0;
        this.Name = guestName;
        this.IsGuest = true;
    }

    internal AccountHandle(int id, string name, string? serial, string? ip)
    {
        this.Id = id;
        this.Name = name;
        this.IsGuest = false;
        this.Serial = serial;
        this.Ip = ip;
    }
}
