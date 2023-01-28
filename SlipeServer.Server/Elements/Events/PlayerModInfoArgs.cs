using SlipeServer.Packets.Definitions.Lua;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerModInfoArgs : EventArgs
{
    public string InfoType { get; }
    public IEnumerable<ModInfoItem> ModInfoItems { get; }

    public PlayerModInfoArgs(string infoType, IEnumerable<ModInfoItem> modInfoItems)
    {
        this.InfoType = infoType;
        this.ModInfoItems = modInfoItems;
    }
}
