using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class WeaponRemovedEventArgs(Ped ped, WeaponId weaponId, ushort? ammoCount) : EventArgs
{
    public Ped Ped { get; } = ped;
    public WeaponId WeaponId { get; } = weaponId;
    public ushort? AmmoCount { get; } = ammoCount == 0 ? null : ammoCount;
}
