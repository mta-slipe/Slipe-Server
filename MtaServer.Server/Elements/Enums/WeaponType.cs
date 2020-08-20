using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements
{
    public enum WeaponType
    {
        WEAPONTYPE_UNARMED = 0,
        WEAPONTYPE_BRASSKNUCKLE,
        WEAPONTYPE_GOLFCLUB,
        WEAPONTYPE_NIGHTSTICK,
        WEAPONTYPE_KNIFE,
        WEAPONTYPE_BASEBALLBAT,
        WEAPONTYPE_SHOVEL,
        WEAPONTYPE_POOL_CUE,
        WEAPONTYPE_KATANA,
        WEAPONTYPE_CHAINSAW,

        // gifts
        WEAPONTYPE_DILDO1,            // 10
        WEAPONTYPE_DILDO2,
        WEAPONTYPE_VIBE1,
        WEAPONTYPE_VIBE2,
        WEAPONTYPE_FLOWERS,
        WEAPONTYPE_CANE,

        WEAPONTYPE_GRENADE,
        WEAPONTYPE_TEARGAS,
        WEAPONTYPE_MOLOTOV,
        WEAPONTYPE_ROCKET,
        WEAPONTYPE_ROCKET_HS,            // 20
        WEAPONTYPE_FREEFALL_BOMB,

        // FIRST SKILL WEAPON
        WEAPONTYPE_PISTOL,            // handguns
        WEAPONTYPE_PISTOL_SILENCED,
        WEAPONTYPE_DESERT_EAGLE,
        WEAPONTYPE_SHOTGUN,                    // shotguns
        WEAPONTYPE_SAWNOFF_SHOTGUN,            // one handed
        WEAPONTYPE_SPAS12_SHOTGUN,
        WEAPONTYPE_MICRO_UZI,            // submachine guns
        WEAPONTYPE_MP5,
        WEAPONTYPE_AK47,            // 30      // machine guns
        WEAPONTYPE_M4,
        WEAPONTYPE_TEC9,            // this uses stat from the micro_uzi
                                    // END SKILL WEAPONS

        WEAPONTYPE_COUNTRYRIFLE,            // rifles
        WEAPONTYPE_SNIPERRIFLE,
        WEAPONTYPE_ROCKETLAUNCHER,            // specials
        WEAPONTYPE_ROCKETLAUNCHER_HS,
        WEAPONTYPE_FLAMETHROWER,
        WEAPONTYPE_MINIGUN,
        WEAPONTYPE_REMOTE_SATCHEL_CHARGE,
        WEAPONTYPE_DETONATOR,            // 40 // plastic explosive
        WEAPONTYPE_SPRAYCAN,
        WEAPONTYPE_EXTINGUISHER,
        WEAPONTYPE_CAMERA,
        WEAPONTYPE_NIGHTVISION,
        WEAPONTYPE_INFRARED,
        WEAPONTYPE_PARACHUTE,
        WEAPONTYPE_LAST_WEAPONTYPE,

        WEAPONTYPE_ARMOUR,
        // these are possible ways to die
        WEAPONTYPE_RAMMEDBYCAR,
        WEAPONTYPE_RUNOVERBYCAR,            // 50
        WEAPONTYPE_EXPLOSION,
        WEAPONTYPE_UZI_DRIVEBY,
        WEAPONTYPE_DROWNING,
        WEAPONTYPE_FALL,
        WEAPONTYPE_UNIDENTIFIED,            // Used for damage being done
        WEAPONTYPE_ANYMELEE,
        WEAPONTYPE_ANYWEAPON,
        WEAPONTYPE_FLARE,

        // Added by us
        WEAPONTYPE_TANK_GRENADE,
        WEAPONTYPE_INVALID = 255,
    }
}
