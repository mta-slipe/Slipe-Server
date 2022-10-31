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

    public static Dictionary<VehicleUpgradeLamp, ushort[]> LampIds { get; } = new()
    {
        [VehicleUpgradeLamp.SquareFog] = new ushort[] { 1024 },
        [VehicleUpgradeLamp.RoundFog] = new ushort[] { 1013 },
    };

    public static Dictionary<VehicleUpgradeRoof, ushort[]> RoofIds { get; } = new()
    {
        [VehicleUpgradeRoof.XFlowRoofVent] = new ushort[] {  1033, 1035 },
        [VehicleUpgradeRoof.XFlow] = new ushort[] {  1053, 1061, 1068, 1091 },
        [VehicleUpgradeRoof.VinylHardtop] = new ushort[] { 1128 },
        [VehicleUpgradeRoof.Softtop] = new ushort[] {  1131 },
        [VehicleUpgradeRoof.RoofScoop] = new ushort[] { 1006 },
        [VehicleUpgradeRoof.Hardtop] = new ushort[] {  1130 },
        [VehicleUpgradeRoof.Covertible] = new ushort[] {  1103 },
        [VehicleUpgradeRoof.AlienRoofVent] = new ushort[] { 1032, 1038 },
        [VehicleUpgradeRoof.Alien] = new ushort[] { 1054, 1055, 1067, 1088 },
    };

    public static Dictionary<VehicleUpgradeNitro, ushort[]> NitroIds { get; } = new()
    {
        [VehicleUpgradeNitro.x2]= new ushort[] { 1009 },
        [VehicleUpgradeNitro.x5]= new ushort[] { 1008 },
        [VehicleUpgradeNitro.x10]= new ushort[] { 1010 },
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

    public static Dictionary<VehicleUpgradeExhaust, ushort[]> ExhaustUpgradeIds { get; } = new()
    {
        [VehicleUpgradeExhaust.XFlow] = new ushort[] { 1029, 1037, 1045, 1059, 1066, 1089 },
        [VehicleUpgradeExhaust.Upswept] = new ushort[] { 1018 },
        [VehicleUpgradeExhaust.Twin] = new ushort[] { 1019 },
        [VehicleUpgradeExhaust.Small] = new ushort[] { 1022 },
        [VehicleUpgradeExhaust.Slamin] = new ushort[] { 1127, 1043, 1105, 1114, 1132, 1135 },
        [VehicleUpgradeExhaust.Medium] = new ushort[] { 1021 },
        [VehicleUpgradeExhaust.Large] = new ushort[] { 1020 },
        [VehicleUpgradeExhaust.Chrome] = new ushort[] { 1126, 1044, 1104, 1113, 1129, 1136 },
        [VehicleUpgradeExhaust.Alien] = new ushort[] { 1028, 1034, 1046, 1064, 1065, 1092 },
    };

    public static Dictionary<VehicleUpgradeFrontBumper, ushort[]> FrontBumperUpgradeIds { get; } = new()
    {
        [VehicleUpgradeFrontBumper.Chrome] = new ushort[] { 1117, 1174, 1176, 1179, 1182, 1189, 1191 },
        [VehicleUpgradeFrontBumper.XFlow] = new ushort[] { 1152, 1157, 1165, 1170, 1172, 1173 },
        [VehicleUpgradeFrontBumper.Alien] = new ushort[] { 1153, 1155, 1160, 1166, 1169, 1171 },
        [VehicleUpgradeFrontBumper.Slamin] = new ushort[] { 1181, 1185, 1188, 1190 },
    };

    public static Dictionary<VehicleUpgradeRearBumper, ushort[]> RearBumperUpgradeIds { get; } = new()
    {
        [VehicleUpgradeRearBumper.XFlow] = new ushort[] { 1140, 1148, 1151, 1156, 1161, 1167, },
        [VehicleUpgradeRearBumper.Alien] = new ushort[] { 1141, 1149, 1150, 1154, 1159, 1168, },
        [VehicleUpgradeRearBumper.Slamin] = new ushort[] { 1175, 1177, 1178, 1183, 1186, 1193 },
        [VehicleUpgradeRearBumper.Chrome] = new ushort[] { 1180, 1184, 1187, 1192 },
    };

    public static Dictionary<VehicleUpgradeMisc, ushort[]> MiscUpgradeIds { get; } = new()
    {
        [VehicleUpgradeMisc.ChromeGrill] = new ushort[] { 1100 },
        [VehicleUpgradeMisc.BullbarChromeLights] = new ushort[] { 1123 },
        [VehicleUpgradeMisc.BullbarChromeBars] = new ushort[] { 1125 },
    };

    public const ushort HydraulicsId = 1087;
    public const ushort StereoId = 1086;

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

        else if (upgradeType == typeof(VehicleUpgradeLamp))
            upgrades = LampIds[(VehicleUpgradeLamp)upgrade];

        else if (upgradeType == typeof(VehicleUpgradeRoof))
            upgrades = RoofIds[(VehicleUpgradeRoof)upgrade];

        else if (upgradeType == typeof(VehicleUpgradeNitro))
            upgrades = NitroIds[(VehicleUpgradeNitro)upgrade];

        else if (upgradeType == typeof(VehicleUpgradeWheel))
            upgrades = WheelUpgradeIds[(VehicleUpgradeWheel)upgrade];

        else if (upgradeType == typeof(VehicleUpgradeExhaust))
            upgrades = ExhaustUpgradeIds[(VehicleUpgradeExhaust)upgrade];

        else if (upgradeType == typeof(VehicleUpgradeFrontBumper))
            upgrades = FrontBumperUpgradeIds[(VehicleUpgradeFrontBumper)upgrade];

        else if (upgradeType == typeof(VehicleUpgradeRearBumper))
            upgrades = RearBumperUpgradeIds[(VehicleUpgradeRearBumper)upgrade];

        else if (upgradeType == typeof(VehicleUpgradeMisc))
            upgrades = MiscUpgradeIds[(VehicleUpgradeMisc)upgrade];

        var result = upgrades.Intersect(VehicleUpgradesPerModel.AvailiableUpgradesPerVehicleModel[model]);
        return result.Any() ? result.First() : null;
    }

    public static ushort? GetUpgradeIdForVehicle<TEnum>(ushort model, ushort upgrade) where TEnum : struct, Enum
    {
        return GetUpgradeIdForVehicle(typeof(TEnum), model, upgrade);
    }
}
