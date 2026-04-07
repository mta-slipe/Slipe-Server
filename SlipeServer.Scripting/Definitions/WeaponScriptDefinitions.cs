using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting.Definitions;

public class WeaponScriptDefinitions(IWeaponConfigurationService weaponConfigurationService)
{
    private readonly static Dictionary<int, string> weaponNames = new()
    {
        [0] = "fist",
        [1] = "brassknuckle",
        [2] = "golfclub",
        [3] = "nightstick",
        [4] = "knife",
        [5] = "bat",
        [6] = "shovel",
        [7] = "poolstick",
        [8] = "katana",
        [9] = "chainsaw",
        [10] = "dildo",
        [11] = "dildo",
        [12] = "vibrator",
        [13] = "vibrator",
        [14] = "flower",
        [15] = "cane",
        [16] = "grenade",
        [17] = "teargas",
        [18] = "molotov",
        [22] = "colt 45",
        [23] = "silenced",
        [24] = "deagle",
        [25] = "shotgun",
        [26] = "sawed-off",
        [27] = "combat shotgun",
        [28] = "uzi",
        [29] = "mp5",
        [30] = "ak-47",
        [31] = "m4",
        [32] = "tec-9",
        [33] = "rifle",
        [34] = "sniper",
        [35] = "rocket launcher",
        [36] = "rocket launcher hs",
        [37] = "flamethrower",
        [38] = "minigun",
        [39] = "satchel",
        [40] = "bomb",
        [41] = "spraycan",
        [42] = "fire extinguisher",
        [43] = "camera",
        [44] = "nightvision",
        [45] = "infrared",
        [46] = "parachute",
    };

    private readonly static Dictionary<string, int> weaponIdsByName =
        BuildWeaponIdsByName();

    private static Dictionary<string, int> BuildWeaponIdsByName()
    {
        return weaponNames.ToDictionary(x => x.Value, x => x.Key);
    }

    [ScriptFunctionDefinition("giveWeapon")]
    public bool GiveWeapon(Ped ped, int weapon, int ammo = 30, bool setAsCurrent = false)
    {
        ped.AddWeapon((WeaponId)weapon, (ushort)ammo, setAsCurrent);
        if (setAsCurrent)
            ped.CurrentWeaponSlot = WeaponConstants.SlotPerWeapon[(WeaponId)weapon];
        return true;
    }

    [ScriptFunctionDefinition("takeAllWeapons")]
    public bool TakeAllWeapons(Ped ped)
    {
        foreach (var weapon in ped.Weapons.ToArray())
            ped.RemoveWeapon(weapon.Type);
        return true;
    }

    [ScriptFunctionDefinition("takeWeapon")]
    public bool TakeWeapon(Ped ped, int weaponId, int? ammo = null)
    {
        ped.RemoveWeapon((WeaponId)weaponId, ammo.HasValue ? (ushort)ammo.Value : null);
        return true;
    }

    [ScriptFunctionDefinition("getWeaponIDFromName")]
    public int GetWeaponIDFromName(string name)
        => weaponIdsByName.TryGetValue(name, out var id) ? id : -1;

    [ScriptFunctionDefinition("getWeaponNameFromID")]
    public string GetWeaponNameFromID(int id)
        => weaponNames.TryGetValue(id, out var name) ? name : "unknown";

    [ScriptFunctionDefinition("getSlotFromWeapon")]
    public int GetSlotFromWeapon(int weaponId)
    {
        if (WeaponConstants.SlotPerWeapon.TryGetValue((WeaponId)weaponId, out var slot))
            return (int)slot;

        return -1;
    }

    [ScriptFunctionDefinition("getWeaponProperty")]
    public object GetWeaponProperty(object weaponIdOrName, string weaponSkill, string property)
    {
        var weaponId = ResolveWeaponId(weaponIdOrName);
        var skill = ParseSkillLevel(weaponSkill);
        var config = weaponConfigurationService.GetWeaponConfiguration(weaponId, skill);
        return GetConfigurationProperty(config, property);
    }

    [ScriptFunctionDefinition("getOriginalWeaponProperty")]
    public object GetOriginalWeaponProperty(object weaponIdOrName, string weaponSkill, string property)
    {
        var weaponId = ResolveWeaponId(weaponIdOrName);
        var skill = ParseSkillLevel(weaponSkill);

        if (!WeaponConfigurationConstants.Defaults.TryGetValue(weaponId, out var skillMap) ||
            !skillMap.TryGetValue(skill, out var config))
            throw new ArgumentException($"No default configuration found for weapon {weaponId} at skill {skill}");

        return GetConfigurationProperty(config, property);
    }

    [ScriptFunctionDefinition("setWeaponAmmo")]
    public bool SetWeaponAmmo(Ped ped, int weapon, int totalAmmo, int ammoInClip = 0)
    {
        ped.SetAmmoCount((WeaponId)weapon, (ushort)totalAmmo, ammoInClip > 0 ? (ushort)ammoInClip : null);
        return true;
    }

    [ScriptFunctionDefinition("setWeaponProperty")]
    public bool SetWeaponProperty(object weaponIdOrName, string weaponSkill, string property, object value)
    {
        var weaponId = ResolveWeaponId(weaponIdOrName);
        var skill = ParseSkillLevel(weaponSkill);
        var config = weaponConfigurationService.GetWeaponConfiguration(weaponId, skill);
        config = SetConfigurationProperty(config, property, value);
        weaponConfigurationService.SetWeaponConfiguration(weaponId, config, skill);
        return true;
    }

    private static WeaponId ResolveWeaponId(object weaponIdOrName)
    {
        if (weaponIdOrName is string name)
        {
            if (!weaponIdsByName.TryGetValue(name, out var id))
                throw new ArgumentException($"Unknown weapon name: {name}");
            return (WeaponId)id;
        }

        var numericId = Convert.ToInt32(weaponIdOrName);
        return (WeaponId)numericId;
    }

    private static WeaponSkillLevel ParseSkillLevel(string skill) => skill.ToLowerInvariant() switch
    {
        "poor" => WeaponSkillLevel.Poor,
        "std" => WeaponSkillLevel.Std,
        "pro" => WeaponSkillLevel.Pro,
        _ => throw new ArgumentException($"Unknown weapon skill level: {skill}")
    };

    private static object GetConfigurationProperty(WeaponConfiguration config, string property) =>
        property.ToLowerInvariant() switch
        {
            "weapon_range" => config.WeaponRange,
            "target_range" => config.TargetRange,
            "accuracy" => config.Accuracy,
            "damage" => (object)config.Damage,
            "life_span" => config.LifeSpan,
            "firing_speed" => config.FiringSpeed,
            "spread" => config.Spread,
            "max_clip_ammo" => (object)config.MaximumClipAmmo,
            "move_speed" => config.MoveSpeed,
            "flags" => (object)config.Flags,
            "anim_group" => (object)config.AnimationGroup,
            "fire_type" => (object)(int)config.FireType,
            "model" => (object)config.Model,
            "model2" => (object)config.Model2,
            "slot" => (object)(int)config.WeaponSlot,
            "skill_level" => (object)(int)config.SkillLevel,
            "required_skill_level" => (object)config.RequiredSkillLevelStat,
            "anim_loop_start" => config.AnimationLoopStart,
            "anim_loop_stop" => config.AnimationLoopStop,
            "anim_loop_release_bullet_time" => config.AnimationLoopBulletFire,
            "anim2_loop_start" => config.Animation2LoopStart,
            "anim2_loop_stop" => config.Animation2LoopStop,
            "anim2_loop_release_bullet_time" => config.Animation2LoopBulletFire,
            "anim_breakout_time" => config.AnimationBreakoutTime,
            "speed" => config.FiringSpeed,
            "radius" => config.Radius,
            "aim_offset" => (object)(int)config.AimOffset,
            "default_combo" => (object)(int)config.DefaultCombo,
            "combos_available" => (object)(int)config.CombosAvailable,
            _ => throw new ArgumentException($"Unknown weapon property: {property}")
        };

    private static WeaponConfiguration SetConfigurationProperty(WeaponConfiguration config, string property, object value)
    {
        var floatValue = Convert.ToSingle(value);
        var intValue = Convert.ToInt32(value);
        var shortValue = (short)intValue;

        switch (property.ToLowerInvariant())
        {
            case "weapon_range": config.WeaponRange = floatValue; break;
            case "target_range": config.TargetRange = floatValue; break;
            case "accuracy": config.Accuracy = floatValue; break;
            case "damage": config.Damage = shortValue; break;
            case "life_span": config.LifeSpan = floatValue; break;
            case "firing_speed": config.FiringSpeed = floatValue; break;
            case "spread": config.Spread = floatValue; break;
            case "max_clip_ammo": config.MaximumClipAmmo = shortValue; break;
            case "move_speed": config.MoveSpeed = floatValue; break;
            case "flags": config.Flags = intValue; break;
            case "anim_group": config.AnimationGroup = (ulong)intValue; break;
            case "fire_type": config.FireType = (WeaponFireType)intValue; break;
            case "model": config.Model = intValue; break;
            case "model2": config.Model2 = intValue; break;
            case "skill_level": config.SkillLevel = (WeaponSkillLevel)intValue; break;
            case "required_skill_level": config.RequiredSkillLevelStat = intValue; break;
            case "anim_loop_start": config.AnimationLoopStart = floatValue; break;
            case "anim_loop_stop": config.AnimationLoopStop = floatValue; break;
            case "anim_loop_release_bullet_time": config.AnimationLoopBulletFire = floatValue; break;
            case "anim2_loop_start": config.Animation2LoopStart = floatValue; break;
            case "anim2_loop_stop": config.Animation2LoopStop = floatValue; break;
            case "anim2_loop_release_bullet_time": config.Animation2LoopBulletFire = floatValue; break;
            case "anim_breakout_time": config.AnimationBreakoutTime = floatValue; break;
            case "speed": config.FiringSpeed = floatValue; break;
            case "radius": config.Radius = floatValue; break;
            case "aim_offset": config.AimOffset = shortValue; break;
            case "default_combo": config.DefaultCombo = (byte)intValue; break;
            case "combos_available": config.CombosAvailable = (byte)intValue; break;
            default: throw new ArgumentException($"Unknown weapon property: {property}");
        }

        return config;
    }
}
