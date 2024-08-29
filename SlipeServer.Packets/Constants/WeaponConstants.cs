using System.Collections.Generic;

namespace SlipeServer.Packets.Constants;

public static class WeaponConstants
{
    public static HashSet<int> WeaponsWithAmmo { get; } =
    [
        2,
        3,
        4,
        5,
        6,
        7,
        8,
        9
    ];

    public static HashSet<byte> SlotsWithAmmo { get; } =
    [
        2,
        3,
        4,
        5,
        6,
        7,
        8,
        9
    ];
}
