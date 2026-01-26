using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class WeaponReceivedEventArgs(Ped ped, WeaponId weaponId, ushort ammoCount, bool setAsCurrent) : EventArgs
{
    public Ped Ped { get; } = ped;
    public WeaponId WeaponId { get; } = weaponId;
    public ushort AmmoCount { get; } = ammoCount;
    public bool SetAsCurrent { get; } = setAsCurrent;
}
