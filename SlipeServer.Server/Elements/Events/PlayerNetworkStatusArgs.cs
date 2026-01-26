using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerNetworkStatusArgs(PlayerNetworkStatusType playerNetworkStatus, uint ticks) : EventArgs
{
    public PlayerNetworkStatusType PlayerNetworkStatus { get; } = playerNetworkStatus;
    public uint Ticks { get; } = ticks;
}
