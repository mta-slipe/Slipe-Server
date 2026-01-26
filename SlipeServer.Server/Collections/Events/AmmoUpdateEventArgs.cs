using SlipeServer.Server.Elements.Structs;
using System;

namespace SlipeServer.Server.Collections.Events;

public sealed class AmmoUpdateEventArgs(Weapon weapon, ushort ammoCount, ushort? ammoInClipCount) : EventArgs
{
    public Weapon Weapon { get; } = weapon;
    public ushort AmmoCount { get; } = ammoCount;
    public ushort? AmmoInClipCount { get; } = ammoInClipCount;
}
