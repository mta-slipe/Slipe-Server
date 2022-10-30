using SlipeServer.Packets.Enums.VehicleUpgrades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Constants;

public class VehicleUpgradeConstants
{
    public static Dictionary<VehicleUpgradeHood, ushort[]> HoodUpgradeIds { get; } = new()
    {
        [VehicleUpgradeHood.LeftOval] = new ushort[] { 1142 },
        [VehicleUpgradeHood.RightOval] = new ushort[] { 1143 },
        [VehicleUpgradeHood.LeftSquare] = new ushort[] { 1144 },
        [VehicleUpgradeHood.RightSquare] = new ushort[] { 1145 },
    };

    public static Dictionary<VehicleUpgradeSpoiler, ushort[]> SpoilerUpgradeIds { get; } = new()
    {
        [VehicleUpgradeSpoiler.XFlow] = new ushort[] { 1050, 1060, 1139, 1146, 1158, 1163 },
        [VehicleUpgradeSpoiler.Worx] = new ushort[] { 1016 },
        [VehicleUpgradeSpoiler.Win] = new ushort[] { 1001 },
        [VehicleUpgradeSpoiler.Race] = new ushort[] { 1015 },
        [VehicleUpgradeSpoiler.Pro] = new ushort[] { 1000 },
        [VehicleUpgradeSpoiler.Fury] = new ushort[] { 1023 },
        [VehicleUpgradeSpoiler.Drag] = new ushort[] { 1002 },
        [VehicleUpgradeSpoiler.Champ] = new ushort[] { 1014 },
        [VehicleUpgradeSpoiler.Alpha] = new ushort[] { 1003 },
        [VehicleUpgradeSpoiler.Alien] = new ushort[] { 1049, 1058, 1138, 1147, 1162, 1164 },
    };

    public static Dictionary<VehicleUpgradeWheel, ushort[]> WheelUpgradeIds { get; } = new()
    {
        [VehicleUpgradeWheel.Wires] = new ushort[] { 1076 },
        [VehicleUpgradeWheel.Virtual] = new ushort[] { 1097 },
        [VehicleUpgradeWheel.Twist] = new ushort[] { 1078 },
        [VehicleUpgradeWheel.Trance] = new ushort[] { 1084 },
        [VehicleUpgradeWheel.Switch] = new ushort[] { 1080 },
        [VehicleUpgradeWheel.Shadow] = new ushort[] { 1073 },
        [VehicleUpgradeWheel.Rimshine] = new ushort[] { 1075 },
        [VehicleUpgradeWheel.Offroad] = new ushort[] { 1025 },
        [VehicleUpgradeWheel.Mega] = new ushort[] { 1074 },
        [VehicleUpgradeWheel.Import] = new ushort[] { 1082 },
        [VehicleUpgradeWheel.Grove] = new ushort[] { 1081 },
        [VehicleUpgradeWheel.Dollar] = new ushort[] { 1083 },
        [VehicleUpgradeWheel.Cutter] = new ushort[] { 1079 },
        [VehicleUpgradeWheel.Classic] = new ushort[] { 1077 },
        [VehicleUpgradeWheel.Atomic] = new ushort[] { 1085 },
        [VehicleUpgradeWheel.Ahab] = new ushort[] { 1096 },
        [VehicleUpgradeWheel.Access] = new ushort[] { 1098 },
    };

    public static ushort? GetUpgradeIdForVehicle(Type upgradeType, ushort model, ushort upgrade)
    {
        ushort[] upgrades = Array.Empty<ushort>();

        if (upgradeType == typeof(VehicleUpgradeHood))
            upgrades = HoodUpgradeIds[(VehicleUpgradeHood)upgrade];
        else if (upgradeType == typeof(VehicleUpgradeSpoiler))
            upgrades = SpoilerUpgradeIds[(VehicleUpgradeSpoiler)upgrade];
        else if (upgradeType == typeof(VehicleUpgradeWheel))
            upgrades = WheelUpgradeIds[(VehicleUpgradeWheel)upgrade];

        var result = upgrades.Intersect(VehicleUpgradesPerModel.AvailiableUpgradesPerVehicleModel[model]);
        return result.Single();
    }

    public static ushort? GetUpgradeIdForVehicle<TEnum>(ushort model, ushort upgrade) where TEnum : struct, Enum
    {
        return GetUpgradeIdForVehicle(typeof(TEnum), model, upgrade);
    }
}
