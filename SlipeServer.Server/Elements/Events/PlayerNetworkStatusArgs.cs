using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerNetworkStatusArgs : EventArgs
{
    public PlayerNetworkStatusType PlayerNetworkStatus { get; }
    public uint Ticks { get; }

    public PlayerNetworkStatusArgs(PlayerNetworkStatusType playerNetworkStatus, uint ticks)
    {
        this.PlayerNetworkStatus = playerNetworkStatus;
        this.Ticks = ticks;
    }
}
