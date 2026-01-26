using SlipeServer.Packets.Definitions.Lua;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerModInfoArgs(string infoType, IEnumerable<ModInfoItem> modInfoItems) : EventArgs
{
    public string InfoType { get; } = infoType;
    public IEnumerable<ModInfoItem> ModInfoItems { get; } = modInfoItems;
}
