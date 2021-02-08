using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Constants
{
    public class WeaponConstants
    {
        public static Dictionary<WeaponId, WeaponSlot> SlotPerWeapon = new Dictionary<WeaponId, WeaponSlot>()
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
    }
}
