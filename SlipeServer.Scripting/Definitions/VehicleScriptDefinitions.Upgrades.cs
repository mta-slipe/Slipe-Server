using SlipeServer.Packets.Enums.VehicleUpgrades;
using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting.Definitions;

public partial class VehicleScriptDefinitions
{
    [ScriptFunctionDefinition("addVehicleUpgrade")]
    public bool AddVehicleUpgrade(Vehicle vehicle, int upgradeId)
        => ApplyUpgradeById(vehicle, (ushort)upgradeId);

    [ScriptFunctionDefinition("removeVehicleUpgrade")]
    public bool RemoveVehicleUpgrade(Vehicle vehicle, int upgradeId)
        => RemoveUpgradeById(vehicle, (ushort)upgradeId);

    [ScriptFunctionDefinition("getVehicleUpgrades")]
    public IEnumerable<ushort> GetVehicleUpgrades(Vehicle vehicle)
        => GetCurrentUpgradeIds(vehicle);

    [ScriptFunctionDefinition("getVehicleUpgradeOnSlot")]
    public int GetVehicleUpgradeOnSlot(Vehicle vehicle, int slot)
        => GetUpgradeIdOnSlot(vehicle.Upgrades, (VehicleUpgradeSlot)slot, vehicle.Model) ?? 0;

    [ScriptFunctionDefinition("getVehicleCompatibleUpgrades")]
    public IEnumerable<ushort> GetVehicleCompatibleUpgrades(Vehicle vehicle, int? slot = null)
    {
        if (!VehicleUpgradesPerModel.AvailiableUpgradesPerVehicleModel.TryGetValue(vehicle.Model, out var available))
            return [];

        if (!slot.HasValue)
            return available;

        return GetCompatibleUpgradesForSlot(vehicle.Model, available, (VehicleUpgradeSlot)slot.Value);
    }

    [ScriptFunctionDefinition("getVehicleUpgradeSlotName")]
    public string? GetVehicleUpgradeSlotName(int slotOrUpgradeId)
    {
        int slot;
        if (slotOrUpgradeId >= 1000)
        {
            if (slotOrUpgradeId == VehicleUpgradeConstants.HydraulicsId)
                slot = (int)VehicleUpgradeSlot.Hydraulics;
            else if (slotOrUpgradeId == VehicleUpgradeConstants.StereoId)
                slot = (int)VehicleUpgradeSlot.Stereo;
            else if (!upgradeIdToSlot.TryGetValue((ushort)slotOrUpgradeId, out var info))
                return null;
            else
                slot = (int)info.slot;
        } else
        {
            slot = slotOrUpgradeId;
        }

        return slot switch
        {
            0 => "hood",
            1 => "vent",
            2 => "spoiler",
            3 => "sideskirt",
            4 => "frontbullbar",
            5 => "rearbullbar",
            6 => "lamps",
            7 => "roof",
            8 => "nitro",
            9 => "hydraulics",
            10 => "stereo",
            11 => "unknown",
            12 => "wheels",
            13 => "exhaust",
            14 => "frontbumper",
            15 => "rearbumper",
            16 => "misc",
            _ => null
        };
    }

    private static bool ApplyUpgradeById(Vehicle vehicle, ushort upgradeId)
    {
        if (upgradeId == VehicleUpgradeConstants.HydraulicsId)
        {
            vehicle.Upgrades.HasHydraulics = true;
            return true;
        }
        if (upgradeId == VehicleUpgradeConstants.StereoId)
        {
            vehicle.Upgrades.HasStereo = true;
            return true;
        }

        if (!VehicleUpgradesPerModel.AvailiableUpgradesPerVehicleModel.TryGetValue(vehicle.Model, out var available)
            || !available.Contains(upgradeId))
            return false;

        if (!upgradeIdToSlot.TryGetValue(upgradeId, out var info))
            return false;

        switch (info.slot)
        {
            case VehicleUpgradeSlot.Hood: vehicle.Upgrades.Hood = (VehicleUpgradeHood)info.enumValue; break;
            case VehicleUpgradeSlot.Vent: vehicle.Upgrades.Vent = (VehicleUpgradeVent)info.enumValue; break;
            case VehicleUpgradeSlot.Spoiler: vehicle.Upgrades.Spoiler = (VehicleUpgradeSpoiler)info.enumValue; break;
            case VehicleUpgradeSlot.Sideskirt: vehicle.Upgrades.Sideskirt = (VehicleUpgradeSideskirt)info.enumValue; break;
            case VehicleUpgradeSlot.FrontBullbars: vehicle.Upgrades.FrontBullbar = (VehicleUpgradeFrontBullbar)info.enumValue; break;
            case VehicleUpgradeSlot.RearBullbars: vehicle.Upgrades.RearBullbar = (VehicleUpgradeRearBullbar)info.enumValue; break;
            case VehicleUpgradeSlot.Lamps: vehicle.Upgrades.Lamps = (VehicleUpgradeLamp)info.enumValue; break;
            case VehicleUpgradeSlot.Roof: vehicle.Upgrades.Roof = (VehicleUpgradeRoof)info.enumValue; break;
            case VehicleUpgradeSlot.Nitro: vehicle.Upgrades.Nitro = (VehicleUpgradeNitro)info.enumValue; break;
            case VehicleUpgradeSlot.Wheels: vehicle.Upgrades.Wheels = (VehicleUpgradeWheel)info.enumValue; break;
            case VehicleUpgradeSlot.Exhaust: vehicle.Upgrades.Exhaust = (VehicleUpgradeExhaust)info.enumValue; break;
            case VehicleUpgradeSlot.FrontBumper: vehicle.Upgrades.FrontBumper = (VehicleUpgradeFrontBumper)info.enumValue; break;
            case VehicleUpgradeSlot.RearBumper: vehicle.Upgrades.RearBumper = (VehicleUpgradeRearBumper)info.enumValue; break;
            case VehicleUpgradeSlot.Misc: vehicle.Upgrades.Misc = (VehicleUpgradeMisc)info.enumValue; break;
            default: return false;
        }
        return true;
    }

    private static bool RemoveUpgradeById(Vehicle vehicle, ushort upgradeId)
    {
        if (upgradeId == VehicleUpgradeConstants.HydraulicsId)
        {
            vehicle.Upgrades.HasHydraulics = false;
            return true;
        }
        if (upgradeId == VehicleUpgradeConstants.StereoId)
        {
            vehicle.Upgrades.HasStereo = false;
            return true;
        }

        if (!upgradeIdToSlot.TryGetValue(upgradeId, out var info))
            return false;

        switch (info.slot)
        {
            case VehicleUpgradeSlot.Hood: vehicle.Upgrades.Hood = VehicleUpgradeHood.None; break;
            case VehicleUpgradeSlot.Vent: vehicle.Upgrades.Vent = VehicleUpgradeVent.None; break;
            case VehicleUpgradeSlot.Spoiler: vehicle.Upgrades.Spoiler = VehicleUpgradeSpoiler.None; break;
            case VehicleUpgradeSlot.Sideskirt: vehicle.Upgrades.Sideskirt = VehicleUpgradeSideskirt.None; break;
            case VehicleUpgradeSlot.FrontBullbars: vehicle.Upgrades.FrontBullbar = VehicleUpgradeFrontBullbar.None; break;
            case VehicleUpgradeSlot.RearBullbars: vehicle.Upgrades.RearBullbar = VehicleUpgradeRearBullbar.None; break;
            case VehicleUpgradeSlot.Lamps: vehicle.Upgrades.Lamps = VehicleUpgradeLamp.None; break;
            case VehicleUpgradeSlot.Roof: vehicle.Upgrades.Roof = VehicleUpgradeRoof.None; break;
            case VehicleUpgradeSlot.Nitro: vehicle.Upgrades.Nitro = VehicleUpgradeNitro.None; break;
            case VehicleUpgradeSlot.Wheels: vehicle.Upgrades.Wheels = VehicleUpgradeWheel.None; break;
            case VehicleUpgradeSlot.Exhaust: vehicle.Upgrades.Exhaust = VehicleUpgradeExhaust.None; break;
            case VehicleUpgradeSlot.FrontBumper: vehicle.Upgrades.FrontBumper = VehicleUpgradeFrontBumper.None; break;
            case VehicleUpgradeSlot.RearBumper: vehicle.Upgrades.RearBumper = VehicleUpgradeRearBumper.None; break;
            case VehicleUpgradeSlot.Misc: vehicle.Upgrades.Misc = VehicleUpgradeMisc.None; break;
            default: return false;
        }
        return true;
    }

    private static IEnumerable<ushort> GetCurrentUpgradeIds(Vehicle vehicle)
    {
        var u = vehicle.Upgrades;
        var result = new List<ushort>();

        if (u.Hood != VehicleUpgradeHood.None)
            AddId(result, VehicleUpgradeConstants.HoodUpgradeIds, u.Hood, vehicle.Model);
        if (u.Vent != VehicleUpgradeVent.None)
            AddId(result, VehicleUpgradeConstants.VentUpgradeIds, u.Vent, vehicle.Model);
        if (u.Spoiler != VehicleUpgradeSpoiler.None)
            AddId(result, VehicleUpgradeConstants.SpoilerUpgradeIds, u.Spoiler, vehicle.Model);
        if (u.Sideskirt != VehicleUpgradeSideskirt.None)
            AddId(result, VehicleUpgradeConstants.SideskirtUpgradeIds, u.Sideskirt, vehicle.Model);
        if (u.FrontBullbar != VehicleUpgradeFrontBullbar.None)
            AddId(result, VehicleUpgradeConstants.FrontBullbarIds, u.FrontBullbar, vehicle.Model);
        if (u.RearBullbar != VehicleUpgradeRearBullbar.None)
            AddId(result, VehicleUpgradeConstants.RearBullbarIds, u.RearBullbar, vehicle.Model);
        if (u.Lamps != VehicleUpgradeLamp.None)
            AddId(result, VehicleUpgradeConstants.LampIds, u.Lamps, vehicle.Model);
        if (u.Roof != VehicleUpgradeRoof.None)
            AddId(result, VehicleUpgradeConstants.RoofIds, u.Roof, vehicle.Model);
        if (u.Nitro != VehicleUpgradeNitro.None)
            AddId(result, VehicleUpgradeConstants.NitroIds, u.Nitro, vehicle.Model);
        if (u.Wheels != VehicleUpgradeWheel.None)
            AddId(result, VehicleUpgradeConstants.WheelUpgradeIds, u.Wheels, vehicle.Model);
        if (u.Exhaust != VehicleUpgradeExhaust.None)
            AddId(result, VehicleUpgradeConstants.ExhaustUpgradeIds, u.Exhaust, vehicle.Model);
        if (u.FrontBumper != VehicleUpgradeFrontBumper.None)
            AddId(result, VehicleUpgradeConstants.FrontBumperUpgradeIds, u.FrontBumper, vehicle.Model);
        if (u.RearBumper != VehicleUpgradeRearBumper.None)
            AddId(result, VehicleUpgradeConstants.RearBumperUpgradeIds, u.RearBumper, vehicle.Model);
        if (u.Misc != VehicleUpgradeMisc.None)
            AddId(result, VehicleUpgradeConstants.MiscUpgradeIds, u.Misc, vehicle.Model);
        if (u.HasHydraulics)
            result.Add(VehicleUpgradeConstants.HydraulicsId);
        if (u.HasStereo)
            result.Add(VehicleUpgradeConstants.StereoId);

        return result;
    }

    private static void AddId<TEnum>(
        List<ushort> result,
        Dictionary<TEnum, ushort[]> ids,
        TEnum value,
        ushort model) where TEnum : struct, Enum
    {
        if (!ids.TryGetValue(value, out var upgradeIds))
            return;
        var available = VehicleUpgradesPerModel.AvailiableUpgradesPerVehicleModel.TryGetValue(model, out var set) ? set : null;
        var id = available != null ? upgradeIds.Intersect(available).FirstOrDefault() : upgradeIds.FirstOrDefault();
        if (id > 0) result.Add(id);
    }

    private static ushort? GetUpgradeIdOnSlot(VehicleUpgrades upgrades, VehicleUpgradeSlot slot, ushort model)
    {
        switch (slot)
        {
            case VehicleUpgradeSlot.Hood when upgrades.Hood != VehicleUpgradeHood.None:
                return GetId(VehicleUpgradeConstants.HoodUpgradeIds, upgrades.Hood, model);
            case VehicleUpgradeSlot.Vent when upgrades.Vent != VehicleUpgradeVent.None:
                return GetId(VehicleUpgradeConstants.VentUpgradeIds, upgrades.Vent, model);
            case VehicleUpgradeSlot.Spoiler when upgrades.Spoiler != VehicleUpgradeSpoiler.None:
                return GetId(VehicleUpgradeConstants.SpoilerUpgradeIds, upgrades.Spoiler, model);
            case VehicleUpgradeSlot.Sideskirt when upgrades.Sideskirt != VehicleUpgradeSideskirt.None:
                return GetId(VehicleUpgradeConstants.SideskirtUpgradeIds, upgrades.Sideskirt, model);
            case VehicleUpgradeSlot.FrontBullbars when upgrades.FrontBullbar != VehicleUpgradeFrontBullbar.None:
                return GetId(VehicleUpgradeConstants.FrontBullbarIds, upgrades.FrontBullbar, model);
            case VehicleUpgradeSlot.RearBullbars when upgrades.RearBullbar != VehicleUpgradeRearBullbar.None:
                return GetId(VehicleUpgradeConstants.RearBullbarIds, upgrades.RearBullbar, model);
            case VehicleUpgradeSlot.Lamps when upgrades.Lamps != VehicleUpgradeLamp.None:
                return GetId(VehicleUpgradeConstants.LampIds, upgrades.Lamps, model);
            case VehicleUpgradeSlot.Roof when upgrades.Roof != VehicleUpgradeRoof.None:
                return GetId(VehicleUpgradeConstants.RoofIds, upgrades.Roof, model);
            case VehicleUpgradeSlot.Nitro when upgrades.Nitro != VehicleUpgradeNitro.None:
                return GetId(VehicleUpgradeConstants.NitroIds, upgrades.Nitro, model);
            case VehicleUpgradeSlot.Hydraulics when upgrades.HasHydraulics:
                return VehicleUpgradeConstants.HydraulicsId;
            case VehicleUpgradeSlot.Stereo when upgrades.HasStereo:
                return VehicleUpgradeConstants.StereoId;
            case VehicleUpgradeSlot.Wheels when upgrades.Wheels != VehicleUpgradeWheel.None:
                return GetId(VehicleUpgradeConstants.WheelUpgradeIds, upgrades.Wheels, model);
            case VehicleUpgradeSlot.Exhaust when upgrades.Exhaust != VehicleUpgradeExhaust.None:
                return GetId(VehicleUpgradeConstants.ExhaustUpgradeIds, upgrades.Exhaust, model);
            case VehicleUpgradeSlot.FrontBumper when upgrades.FrontBumper != VehicleUpgradeFrontBumper.None:
                return GetId(VehicleUpgradeConstants.FrontBumperUpgradeIds, upgrades.FrontBumper, model);
            case VehicleUpgradeSlot.RearBumper when upgrades.RearBumper != VehicleUpgradeRearBumper.None:
                return GetId(VehicleUpgradeConstants.RearBumperUpgradeIds, upgrades.RearBumper, model);
            case VehicleUpgradeSlot.Misc when upgrades.Misc != VehicleUpgradeMisc.None:
                return GetId(VehicleUpgradeConstants.MiscUpgradeIds, upgrades.Misc, model);
            default:
                return null;
        }
    }

    private static ushort? GetId<TEnum>(Dictionary<TEnum, ushort[]> ids, TEnum value, ushort model)
        where TEnum : struct, Enum
    {
        if (!ids.TryGetValue(value, out var upgradeIds))
            return null;
        var available = VehicleUpgradesPerModel.AvailiableUpgradesPerVehicleModel.TryGetValue(model, out var set) ? set : null;
        var id = available != null ? upgradeIds.Intersect(available).FirstOrDefault() : upgradeIds.FirstOrDefault();
        return id > 0 ? id : null;
    }

    private static IEnumerable<ushort> GetCompatibleUpgradesForSlot(ushort model, HashSet<ushort> available, VehicleUpgradeSlot slot)
    {
        IEnumerable<ushort[]> idArrays = slot switch
        {
            VehicleUpgradeSlot.Hood => VehicleUpgradeConstants.HoodUpgradeIds.Values,
            VehicleUpgradeSlot.Vent => VehicleUpgradeConstants.VentUpgradeIds.Values,
            VehicleUpgradeSlot.Spoiler => VehicleUpgradeConstants.SpoilerUpgradeIds.Values,
            VehicleUpgradeSlot.Sideskirt => VehicleUpgradeConstants.SideskirtUpgradeIds.Values,
            VehicleUpgradeSlot.FrontBullbars => VehicleUpgradeConstants.FrontBullbarIds.Values,
            VehicleUpgradeSlot.RearBullbars => VehicleUpgradeConstants.RearBullbarIds.Values,
            VehicleUpgradeSlot.Lamps => VehicleUpgradeConstants.LampIds.Values,
            VehicleUpgradeSlot.Roof => VehicleUpgradeConstants.RoofIds.Values,
            VehicleUpgradeSlot.Nitro => VehicleUpgradeConstants.NitroIds.Values,
            VehicleUpgradeSlot.Hydraulics => available.Contains(VehicleUpgradeConstants.HydraulicsId)
                ? [[VehicleUpgradeConstants.HydraulicsId]] : [],
            VehicleUpgradeSlot.Stereo => available.Contains(VehicleUpgradeConstants.StereoId)
                ? [[VehicleUpgradeConstants.StereoId]] : [],
            VehicleUpgradeSlot.Wheels => VehicleUpgradeConstants.WheelUpgradeIds.Values,
            VehicleUpgradeSlot.Exhaust => VehicleUpgradeConstants.ExhaustUpgradeIds.Values,
            VehicleUpgradeSlot.FrontBumper => VehicleUpgradeConstants.FrontBumperUpgradeIds.Values,
            VehicleUpgradeSlot.RearBumper => VehicleUpgradeConstants.RearBumperUpgradeIds.Values,
            VehicleUpgradeSlot.Misc => VehicleUpgradeConstants.MiscUpgradeIds.Values,
            _ => [],
        };

        return idArrays.SelectMany(x => x).Intersect(available).Distinct();
    }
}
