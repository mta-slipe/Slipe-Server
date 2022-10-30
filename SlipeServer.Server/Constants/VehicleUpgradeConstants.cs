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

    public static Dictionary<VehicleUpgradeVent, ushort[]> VentUpgradeIds { get; } = new()
    {
        [VehicleUpgradeVent.WorxScoop] = new ushort[] { 1012 },
        [VehicleUpgradeVent.RaceScoop] = new ushort[] { 1011 },
        [VehicleUpgradeVent.FuryScoop] = new ushort[] { 1005 },
        [VehicleUpgradeVent.ChampScoop] = new ushort[] { 1004 },
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

    public static Dictionary<VehicleUpgradeSideskirt, ushort[]> SideskirtUpgradeIds { get; } = new()
    {
        [VehicleUpgradeSideskirt.RightXFlow] = new ushort[] { 1031, 1057, 1070, 1093, 1095, 1041, 1048 },
        [VehicleUpgradeSideskirt.Right] = new ushort[] { 1007 },
        [VehicleUpgradeSideskirt.RightAlien] = new ushort[] { 1007, 1026, 1056, 1069, 1090, 1036, 1047 },
        [VehicleUpgradeSideskirt.RightWheelcovers] = new ushort[] { 1119 },
        [VehicleUpgradeSideskirt.RightChromeTrim] = new ushort[] { 1118 },
        [VehicleUpgradeSideskirt.RightChromeStrip] = new ushort[] { 1108, 1133, 1134 },
        [VehicleUpgradeSideskirt.RightChromeFlames] = new ushort[] { 1122 },
        [VehicleUpgradeSideskirt.RightChromeArches] = new ushort[] { 1106 },
        [VehicleUpgradeSideskirt.LeftXFlow] = new ushort[] { 1030, 1063, 1072, 1039, 1052 },
        [VehicleUpgradeSideskirt.Left] = new ushort[] { 1017 },
        [VehicleUpgradeSideskirt.LeftChrome] = new ushort[] { 1099 },
        [VehicleUpgradeSideskirt.LeftAlien] = new ushort[] { 1027, 1062, 1071, 1094, 1040, 1051 },
        [VehicleUpgradeSideskirt.LeftWheelcovers] = new ushort[] { 1121 },
        [VehicleUpgradeSideskirt.LeftChromeTrim] = new ushort[] { 1120 },
        [VehicleUpgradeSideskirt.LeftChromeStrip] = new ushort[] { 1102, 1107, 1137  },
        [VehicleUpgradeSideskirt.LeftChromeFlames] = new ushort[] { 1101 },
        [VehicleUpgradeSideskirt.LeftChromeArches] = new ushort[] { 1124 },
        [VehicleUpgradeSideskirt.RightChrome] = new ushort[] { 1042 },
    };

    public static Dictionary<VehicleUpgradeFrontBullbar, ushort[]> FrontBullbarIds { get; } = new()
    {
        [VehicleUpgradeFrontBullbar.Chrome] = new ushort[] { 1115 },
        [VehicleUpgradeFrontBullbar.Slamin] = new ushort[] { 1116 },
    };

    public static Dictionary<VehicleUpgradeRearBullbar, ushort[]> RearBullbarIds { get; } = new()
    {
        [VehicleUpgradeRearBullbar.Chrome] = new ushort[] { 1109 },
        [VehicleUpgradeRearBullbar.Slamin] = new ushort[] { 1110 },
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

    public static Dictionary<VehicleUpgradeMisc, ushort[]> MiscUpgradeIds { get; } = new()
    {
        [VehicleUpgradeMisc.ChromeGrill] = new ushort[] { 1100 },
        [VehicleUpgradeMisc.BullbarChromeLights] = new ushort[] { 1123 },
        [VehicleUpgradeMisc.BullbarChromeBars] = new ushort[] { 1125 },
    };

    public static ushort? GetUpgradeIdForVehicle(Type upgradeType, ushort model, ushort upgrade)
    {
        ushort[] upgrades = Array.Empty<ushort>();

        if (upgradeType == typeof(VehicleUpgradeHood))
            upgrades = HoodUpgradeIds[(VehicleUpgradeHood)upgrade];
        else if (upgradeType == typeof(VehicleUpgradeVent))
            upgrades = VentUpgradeIds[(VehicleUpgradeVent)upgrade];
        else if (upgradeType == typeof(VehicleUpgradeSpoiler))
            upgrades = SpoilerUpgradeIds[(VehicleUpgradeSpoiler)upgrade];
        else if (upgradeType == typeof(VehicleUpgradeSideskirt))
            upgrades = SideskirtUpgradeIds[(VehicleUpgradeSideskirt)upgrade];
        else if (upgradeType == typeof(VehicleUpgradeFrontBullbar))
            upgrades = FrontBullbarIds[(VehicleUpgradeFrontBullbar)upgrade];
        else if (upgradeType == typeof(VehicleUpgradeRearBullbar))
            upgrades = RearBullbarIds[(VehicleUpgradeRearBullbar)upgrade];
        else if (upgradeType == typeof(VehicleUpgradeWheel))
            upgrades = WheelUpgradeIds[(VehicleUpgradeWheel)upgrade];
        else if (upgradeType == typeof(VehicleUpgradeMisc))
            upgrades = MiscUpgradeIds[(VehicleUpgradeMisc)upgrade];

        var result = upgrades.Intersect(VehicleUpgradesPerModel.AvailiableUpgradesPerVehicleModel[model]);
        return result.Single();
    }

    public static ushort? GetUpgradeIdForVehicle<TEnum>(ushort model, ushort upgrade) where TEnum : struct, Enum
    {
        return GetUpgradeIdForVehicle(typeof(TEnum), model, upgrade);
    }
}
