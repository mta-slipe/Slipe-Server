using SlipeServer.Packets.Enums.VehicleUpgrades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Constants;

public class VehicleUpgradeConstants
{
    public static Dictionary<VehicleUpgradeHood, ushort[]> HoodUpgradeIds { get; } = new()
    {
        [VehicleUpgradeHood.LeftOval] = [1142],
        [VehicleUpgradeHood.RightOval] = [1143],
        [VehicleUpgradeHood.LeftSquare] = [1144],
        [VehicleUpgradeHood.RightSquare] = [1145],
    };

    public static Dictionary<VehicleUpgradeVent, ushort[]> VentUpgradeIds { get; } = new()
    {
        [VehicleUpgradeVent.WorxScoop] = [1012],
        [VehicleUpgradeVent.RaceScoop] = [1011],
        [VehicleUpgradeVent.FuryScoop] = [1005],
        [VehicleUpgradeVent.ChampScoop] = [1004],
    };

    public static Dictionary<VehicleUpgradeSpoiler, ushort[]> SpoilerUpgradeIds { get; } = new()
    {
        [VehicleUpgradeSpoiler.XFlow] = [1050, 1060, 1139, 1146, 1158, 1163],
        [VehicleUpgradeSpoiler.Worx] = [1016],
        [VehicleUpgradeSpoiler.Win] = [1001],
        [VehicleUpgradeSpoiler.Race] = [1015],
        [VehicleUpgradeSpoiler.Pro] = [1000],
        [VehicleUpgradeSpoiler.Fury] = [1023],
        [VehicleUpgradeSpoiler.Drag] = [1002],
        [VehicleUpgradeSpoiler.Champ] = [1014],
        [VehicleUpgradeSpoiler.Alpha] = [1003],
        [VehicleUpgradeSpoiler.Alien] = [1049, 1058, 1138, 1147, 1162, 1164],
    };

    public static Dictionary<VehicleUpgradeSideskirt, ushort[]> SideskirtUpgradeIds { get; } = new()
    {
        [VehicleUpgradeSideskirt.RightXFlow] = [1031, 1057, 1070, 1093, 1095, 1041, 1048],
        [VehicleUpgradeSideskirt.Right] = [1007],
        [VehicleUpgradeSideskirt.RightAlien] = [1007, 1026, 1056, 1069, 1090, 1036, 1047],
        [VehicleUpgradeSideskirt.RightWheelcovers] = [1119],
        [VehicleUpgradeSideskirt.RightChromeTrim] = [1118],
        [VehicleUpgradeSideskirt.RightChromeStrip] = [1108, 1133, 1134],
        [VehicleUpgradeSideskirt.RightChromeFlames] = [1122],
        [VehicleUpgradeSideskirt.RightChromeArches] = [1106],
        [VehicleUpgradeSideskirt.LeftXFlow] = [1030, 1063, 1072, 1039, 1052],
        [VehicleUpgradeSideskirt.Left] = [1017],
        [VehicleUpgradeSideskirt.LeftChrome] = [1099],
        [VehicleUpgradeSideskirt.LeftAlien] = [1027, 1062, 1071, 1094, 1040, 1051],
        [VehicleUpgradeSideskirt.LeftWheelcovers] = [1121],
        [VehicleUpgradeSideskirt.LeftChromeTrim] = [1120],
        [VehicleUpgradeSideskirt.LeftChromeStrip] = [1102, 1107, 1137],
        [VehicleUpgradeSideskirt.LeftChromeFlames] = [1101],
        [VehicleUpgradeSideskirt.LeftChromeArches] = [1124],
        [VehicleUpgradeSideskirt.RightChrome] = [1042],
    };

    public static Dictionary<VehicleUpgradeFrontBullbar, ushort[]> FrontBullbarIds { get; } = new()
    {
        [VehicleUpgradeFrontBullbar.Chrome] = [1115],
        [VehicleUpgradeFrontBullbar.Slamin] = [1116],
    };

    public static Dictionary<VehicleUpgradeRearBullbar, ushort[]> RearBullbarIds { get; } = new()
    {
        [VehicleUpgradeRearBullbar.Chrome] = [1109],
        [VehicleUpgradeRearBullbar.Slamin] = [1110],
    };

    public static Dictionary<VehicleUpgradeLamp, ushort[]> LampIds { get; } = new()
    {
        [VehicleUpgradeLamp.SquareFog] = [1024],
        [VehicleUpgradeLamp.RoundFog] = [1013],
    };

    public static Dictionary<VehicleUpgradeRoof, ushort[]> RoofIds { get; } = new()
    {
        [VehicleUpgradeRoof.XFlowRoofVent] = [1033, 1035],
        [VehicleUpgradeRoof.XFlow] = [1053, 1061, 1068, 1091],
        [VehicleUpgradeRoof.VinylHardtop] = [1128],
        [VehicleUpgradeRoof.Softtop] = [1131],
        [VehicleUpgradeRoof.RoofScoop] = [1006],
        [VehicleUpgradeRoof.Hardtop] = [1130],
        [VehicleUpgradeRoof.Covertible] = [1103],
        [VehicleUpgradeRoof.AlienRoofVent] = [1032, 1038],
        [VehicleUpgradeRoof.Alien] = [1054, 1055, 1067, 1088],
    };

    public static Dictionary<VehicleUpgradeNitro, ushort[]> NitroIds { get; } = new()
    {
        [VehicleUpgradeNitro.x2]= [1009],
        [VehicleUpgradeNitro.x5]= [1008],
        [VehicleUpgradeNitro.x10]= [1010],
    };

    public static Dictionary<VehicleUpgradeWheel, ushort[]> WheelUpgradeIds { get; } = new()
    {
        [VehicleUpgradeWheel.Wires] = [1076],
        [VehicleUpgradeWheel.Virtual] = [1097],
        [VehicleUpgradeWheel.Twist] = [1078],
        [VehicleUpgradeWheel.Trance] = [1084],
        [VehicleUpgradeWheel.Switch] = [1080],
        [VehicleUpgradeWheel.Shadow] = [1073],
        [VehicleUpgradeWheel.Rimshine] = [1075],
        [VehicleUpgradeWheel.Offroad] = [1025],
        [VehicleUpgradeWheel.Mega] = [1074],
        [VehicleUpgradeWheel.Import] = [1082],
        [VehicleUpgradeWheel.Grove] = [1081],
        [VehicleUpgradeWheel.Dollar] = [1083],
        [VehicleUpgradeWheel.Cutter] = [1079],
        [VehicleUpgradeWheel.Classic] = [1077],
        [VehicleUpgradeWheel.Atomic] = [1085],
        [VehicleUpgradeWheel.Ahab] = [1096],
        [VehicleUpgradeWheel.Access] = [1098],
    };

    public static Dictionary<VehicleUpgradeExhaust, ushort[]> ExhaustUpgradeIds { get; } = new()
    {
        [VehicleUpgradeExhaust.XFlow] = [1029, 1037, 1045, 1059, 1066, 1089],
        [VehicleUpgradeExhaust.Upswept] = [1018],
        [VehicleUpgradeExhaust.Twin] = [1019],
        [VehicleUpgradeExhaust.Small] = [1022],
        [VehicleUpgradeExhaust.Slamin] = [1127, 1043, 1105, 1114, 1132, 1135],
        [VehicleUpgradeExhaust.Medium] = [1021],
        [VehicleUpgradeExhaust.Large] = [1020],
        [VehicleUpgradeExhaust.Chrome] = [1126, 1044, 1104, 1113, 1129, 1136],
        [VehicleUpgradeExhaust.Alien] = [1028, 1034, 1046, 1064, 1065, 1092],
    };

    public static Dictionary<VehicleUpgradeFrontBumper, ushort[]> FrontBumperUpgradeIds { get; } = new()
    {
        [VehicleUpgradeFrontBumper.Chrome] = [1117, 1174, 1176, 1179, 1182, 1189, 1191],
        [VehicleUpgradeFrontBumper.XFlow] = [1152, 1157, 1165, 1170, 1172, 1173],
        [VehicleUpgradeFrontBumper.Alien] = [1153, 1155, 1160, 1166, 1169, 1171],
        [VehicleUpgradeFrontBumper.Slamin] = [1181, 1185, 1188, 1190],
    };

    public static Dictionary<VehicleUpgradeRearBumper, ushort[]> RearBumperUpgradeIds { get; } = new()
    {
        [VehicleUpgradeRearBumper.XFlow] = [1140, 1148, 1151, 1156, 1161, 1167,],
        [VehicleUpgradeRearBumper.Alien] = [1141, 1149, 1150, 1154, 1159, 1168,],
        [VehicleUpgradeRearBumper.Slamin] = [1175, 1177, 1178, 1183, 1186, 1193],
        [VehicleUpgradeRearBumper.Chrome] = [1180, 1184, 1187, 1192],
    };

    public static Dictionary<VehicleUpgradeMisc, ushort[]> MiscUpgradeIds { get; } = new()
    {
        [VehicleUpgradeMisc.ChromeGrill] = [1100],
        [VehicleUpgradeMisc.BullbarChromeLights] = [1123],
        [VehicleUpgradeMisc.BullbarChromeBars] = [1125],
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
