using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Constants
{
    public class WeaponConstants
    {
        public static Dictionary<WeaponId, WeaponSlot> SlotPerWeapon { get; } = new ()
        {
            [WeaponId.Fist] = WeaponSlot.Hand,
            [WeaponId.BrassKnuckle] = WeaponSlot.Melee,
            [WeaponId.Golfclub] = WeaponSlot.Melee,
            [WeaponId.Nightstick] = WeaponSlot.Melee,
            [WeaponId.Knife] = WeaponSlot.Melee,
            [WeaponId.Bat] = WeaponSlot.Melee,
            [WeaponId.Shovel] = WeaponSlot.Melee,
            [WeaponId.Poolstick] = WeaponSlot.Melee,
            [WeaponId.Katana] = WeaponSlot.Melee,
            [WeaponId.Chainsaw] = WeaponSlot.Melee,

            [WeaponId.Dildo] = WeaponSlot.Gifts,
            [WeaponId.Dildo2] = WeaponSlot.Gifts,
            [WeaponId.Vibrator] = WeaponSlot.Gifts,
            [WeaponId.Flower] = WeaponSlot.Gifts,
            [WeaponId.Cane] = WeaponSlot.Gifts,

            [WeaponId.Grenade] = WeaponSlot.Projectiles,
            [WeaponId.Teargass] = WeaponSlot.Projectiles,
            [WeaponId.Molotov] = WeaponSlot.Projectiles,

            [WeaponId.Colt] = WeaponSlot.Handguns,
            [WeaponId.Silenced] = WeaponSlot.Handguns,
            [WeaponId.Deagle] = WeaponSlot.Handguns,
            [WeaponId.Shotgun] = WeaponSlot.Shotguns,
            [WeaponId.Sawnoff] = WeaponSlot.Shotguns,
            [WeaponId.CombatShotgun] = WeaponSlot.Shotguns,
            [WeaponId.Uzi] = WeaponSlot.SubMachineGuns,
            [WeaponId.Mp5] = WeaponSlot.SubMachineGuns,
            [WeaponId.Ak47] = WeaponSlot.AssaultRifles,
            [WeaponId.M4] = WeaponSlot.AssaultRifles,
            [WeaponId.Tec9] = WeaponSlot.SubMachineGuns,
            [WeaponId.Rifle] = WeaponSlot.Rifles,
            [WeaponId.Sniper] = WeaponSlot.Rifles,
            [WeaponId.RocketLauncher] = WeaponSlot.HeavyWeapons,
            [WeaponId.HeatSeakingRocketLauncher] = WeaponSlot.HeavyWeapons,
            [WeaponId.Flamethrower] = WeaponSlot.HeavyWeapons,
            [WeaponId.Minigun] = WeaponSlot.HeavyWeapons,
            [WeaponId.Satchel] = WeaponSlot.Projectiles,
            [WeaponId.Detonator] = WeaponSlot.SatchelDetonator,
            [WeaponId.Spraycan] = WeaponSlot.Special1,
            [WeaponId.FireExtinguisher] = WeaponSlot.Special1,
            [WeaponId.Camera] = WeaponSlot.Special1,
            [WeaponId.Nightvision] = WeaponSlot.Special2,
            [WeaponId.Infrared] = WeaponSlot.Special2,
            [WeaponId.Parachute] = WeaponSlot.Special2,
        };

        public static Dictionary<WeaponId, ushort> ClipCountsPerWeapon { get; } = new()
        {
            [WeaponId.Fist] = 1,
            [WeaponId.BrassKnuckle] = 1,
            [WeaponId.Golfclub] = 1,
            [WeaponId.Nightstick] = 1,
            [WeaponId.Knife] = 1,
            [WeaponId.Bat] = 1,
            [WeaponId.Shovel] = 1,
            [WeaponId.Poolstick] = 1,
            [WeaponId.Katana] = 1,
            [WeaponId.Chainsaw] = 1,

            [WeaponId.Dildo] = 1,
            [WeaponId.Dildo2] = 1,
            [WeaponId.Vibrator] = 1,
            [WeaponId.Flower] = 1,
            [WeaponId.Cane] = 1,

            [WeaponId.Grenade] = 1,
            [WeaponId.Teargass] = 1,
            [WeaponId.Molotov] = 1,

            [WeaponId.Colt] = 17,
            [WeaponId.Silenced] = 17,
            [WeaponId.Deagle] = 7,
            [WeaponId.Shotgun] = 1,
            [WeaponId.Sawnoff] = 2,
            [WeaponId.CombatShotgun] = 7,
            [WeaponId.Uzi] = 50,
            [WeaponId.Mp5] = 30,
            [WeaponId.Ak47] = 30,
            [WeaponId.M4] = 50,
            [WeaponId.Tec9] = 50,
            [WeaponId.Rifle] = 1,
            [WeaponId.Sniper] = 1,
            [WeaponId.RocketLauncher] = 1,
            [WeaponId.HeatSeakingRocketLauncher] = 1,
            [WeaponId.Flamethrower] = 500,
            [WeaponId.Minigun] = 500,
            [WeaponId.Satchel] = 1,
            [WeaponId.Detonator] = 1,
            [WeaponId.Spraycan] = 500,
            [WeaponId.FireExtinguisher] = 500,
            [WeaponId.Camera] = 1,
            [WeaponId.Nightvision] = 1,
            [WeaponId.Infrared] = 1,
            [WeaponId.Parachute] = 1,
        };

        public static Dictionary<WeaponId, ushort> ModelsPerWeapon { get; } = new()
        {
            [WeaponId.Fist] = 0,
            [WeaponId.BrassKnuckle] = 331,
            [WeaponId.Golfclub] = 333,
            [WeaponId.Nightstick] = 334,
            [WeaponId.Knife] = 335,
            [WeaponId.Bat] = 336,
            [WeaponId.Shovel] = 337,
            [WeaponId.Poolstick] = 338,
            [WeaponId.Katana] = 339,
            [WeaponId.Chainsaw] = 341,

            [WeaponId.Dildo] = 321,
            [WeaponId.Dildo2] = 322,
            [WeaponId.Vibrator] = 323,
            [WeaponId.Flower] = 325,
            [WeaponId.Cane] = 326,

            [WeaponId.Grenade] = 342,
            [WeaponId.Teargass] = 343,
            [WeaponId.Molotov] = 344,

            [WeaponId.Colt] = 346,
            [WeaponId.Silenced] = 347,
            [WeaponId.Deagle] = 348,
            [WeaponId.Shotgun] = 349,
            [WeaponId.Sawnoff] = 350,
            [WeaponId.CombatShotgun] = 351,
            [WeaponId.Uzi] = 352,
            [WeaponId.Mp5] = 353,
            [WeaponId.Ak47] = 355,
            [WeaponId.M4] = 356,
            [WeaponId.Tec9] = 372,
            [WeaponId.Rifle] = 357,
            [WeaponId.Sniper] = 358,
            [WeaponId.RocketLauncher] = 359,
            [WeaponId.HeatSeakingRocketLauncher] = 360,
            [WeaponId.Flamethrower] = 361,
            [WeaponId.Minigun] = 362,
            [WeaponId.Satchel] = 363,
            [WeaponId.Detonator] = 364,
            [WeaponId.Spraycan] = 365,
            [WeaponId.FireExtinguisher] = 366,
            [WeaponId.Camera] = 367,
            [WeaponId.Nightvision] = 368,
            [WeaponId.Infrared] = 369,
            [WeaponId.Parachute] = 371,
        };

        public static Dictionary<WeaponId, PedStat> WeaponStatsPerWeapon { get; } = new()
        {
            [WeaponId.Colt] = PedStat.WEAPONTYPE_PISTOL_SKILL,
            [WeaponId.Silenced] = PedStat.WEAPONTYPE_PISTOL_SILENCED_SKILL,
            [WeaponId.Deagle] = PedStat.WEAPONTYPE_DESERT_EAGLE_SKILL,
            [WeaponId.Shotgun] = PedStat.WEAPONTYPE_SHOTGUN_SKILL,
            [WeaponId.Sawnoff] = PedStat.WEAPONTYPE_SAWNOFF_SHOTGUN_SKILL,
            [WeaponId.CombatShotgun] = PedStat.WEAPONTYPE_SPAS12_SHOTGUN_SKILL,
            [WeaponId.Uzi] = PedStat.WEAPONTYPE_MICRO_UZI_SKILL,
            [WeaponId.Mp5] = PedStat.WEAPONTYPE_MP5_SKILL,
            [WeaponId.Ak47] = PedStat.WEAPONTYPE_AK47_SKILL,
            [WeaponId.M4] = PedStat.WEAPONTYPE_M4_SKILL,
            [WeaponId.Tec9] = PedStat.WEAPONTYPE_MICRO_UZI_SKILL,
            [WeaponId.Rifle] = PedStat.WEAPONTYPE_SNIPERRIFLE_SKILL,
            [WeaponId.Sniper] = PedStat.WEAPONTYPE_SNIPERRIFLE_SKILL,
        };
    }
}
