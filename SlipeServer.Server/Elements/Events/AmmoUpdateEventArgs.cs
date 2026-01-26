using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class AmmoUpdateEventArgs(Ped ped, WeaponId weaponId, ushort ammoCount, ushort? ammoInClipCount, bool isSync = false) : EventArgs
{
    public Ped Ped { get; set; } = ped;
    public WeaponId WeaponId { get; } = weaponId;
    public ushort AmmoCount { get; } = ammoCount;
    public ushort? AmmoInClipCount { get; } = ammoInClipCount;
    public bool IsSync { get; } = isSync;
}
