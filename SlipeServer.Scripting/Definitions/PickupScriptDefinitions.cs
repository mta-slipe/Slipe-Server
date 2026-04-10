using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class PickupScriptDefinitions(IMtaServer server)
{
    [ScriptFunctionDefinition("createPickup")]
    public Pickup CreatePickup(float x, float y, float z, int type, int amount, int respawnTime = 30000, int ammo = 50)
    {
        var pickupType = (PickupType)type;

        Pickup pickup = pickupType switch
        {
            PickupType.Health => new Pickup(new Vector3(x, y, z), PickupType.Health, amount),
            PickupType.Armor => new Pickup(new Vector3(x, y, z), PickupType.Armor, amount),
            PickupType.Weapon => new Pickup(new Vector3(x, y, z), (WeaponId)amount, (ushort)ammo),
            _ => new Pickup(new Vector3(x, y, z), (ushort)amount),
        };

        pickup.RespawnTime = (uint)respawnTime;
        pickup.AssociateWith(server);

        if (ScriptExecutionContext.Current?.Owner != null)
            pickup.Parent = ScriptExecutionContext.Current.Owner?.DynamicRoot;

        return pickup;
    }

    [ScriptFunctionDefinition("getPickupType")]
    public int GetPickupType(Pickup pickup) => (int)pickup.PickupType;

    [ScriptFunctionDefinition("getPickupAmmo")]
    public int GetPickupAmmo(Pickup pickup) => pickup.Ammo ?? 0;

    [ScriptFunctionDefinition("getPickupAmount")]
    public float GetPickupAmount(Pickup pickup) => pickup.PickupType switch
    {
        PickupType.Health => pickup.Health ?? 0,
        PickupType.Armor => pickup.Armor ?? 0,
        _ => 0,
    };

    [ScriptFunctionDefinition("getPickupWeapon")]
    public int GetPickupWeapon(Pickup pickup) => (int)(pickup.WeaponType ?? 0);

    [ScriptFunctionDefinition("setPickupType")]
    public bool SetPickupType(Pickup pickup, int type, int amount, int ammo = 50)
    {
        switch ((PickupType)type)
        {
            case PickupType.Health:
                pickup.ChangeToOrUpdateHealthPickup(amount);
                break;
            case PickupType.Armor:
                pickup.ChangeToOrUpdateArmorPickup(amount);
                break;
            case PickupType.Weapon:
                pickup.ChangeToOrUpdateWeaponPickup((WeaponId)amount, (ushort)ammo);
                break;
            default:
                pickup.ChangeToOrUpdateCustomPickup((ushort)amount);
                break;
        }
        return true;
    }

    [ScriptFunctionDefinition("getPickupRespawnInterval")]
    public uint GetPickupRespawnInterval(Pickup pickup) => pickup.RespawnTime;

    [ScriptFunctionDefinition("setPickupRespawnInterval")]
    public bool SetPickupRespawnInterval(Pickup pickup, int ms)
    {
        pickup.RespawnTime = (uint)ms;
        return true;
    }

    [ScriptFunctionDefinition("isPickupSpawned")]
    public bool IsPickupSpawned(Pickup pickup) => pickup.IsVisible;

    [ScriptFunctionDefinition("spawnPickup")]
    public bool SpawnPickup(Pickup pickup)
    {
        pickup.Respawn();
        return true;
    }

    [ScriptFunctionDefinition("usePickup")]
    public bool UsePickup(Pickup pickup, Player player)
    {
        pickup.Use(player);
        return true;
    }
}
