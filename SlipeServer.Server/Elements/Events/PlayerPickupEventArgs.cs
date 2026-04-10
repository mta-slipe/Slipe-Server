using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerPickupHitEventArgs(Pickup pickup) : EventArgs
{
    public Pickup Pickup { get; } = pickup;
}

public sealed class PlayerPickupLeftEventArgs(Pickup pickup) : EventArgs
{
    public Pickup Pickup { get; } = pickup;
}

public sealed class PlayerPickupUsedEventArgs(Pickup pickup) : EventArgs
{
    public Pickup Pickup { get; } = pickup;
}
