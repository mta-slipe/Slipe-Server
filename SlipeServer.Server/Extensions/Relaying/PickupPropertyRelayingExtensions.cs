using SlipeServer.Packets.Definitions.Lua.ElementRpc.Pickups;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Extensions.Relaying;

public static class PickupPropertyRelayingExtensions
{
    public static void AddPickupRelayers(this Pickup pickup)
    {
        pickup.PickupTypeChanged += RelayTypeChange;
    }

    private static void RelayTypeChange(Pickup pickup, EventArgs args)
    {
        switch (pickup.PickupType)
        {
            case PickupType.Health:
                pickup.RelayChange(SetPickupTypeRpcPacket.CreateHealth(pickup.Id, pickup.Health ?? 100));
                break;
            case PickupType.Armor:
                pickup.RelayChange(SetPickupTypeRpcPacket.CreateArmor(pickup.Id, pickup.Armor ?? 100));
                break;  
            case PickupType.Weapon:
                pickup.RelayChange(new SetPickupTypeRpcPacket(pickup.Id, (byte)(pickup.WeaponType ?? WeaponId.BrassKnuckle), pickup.Ammo ?? 1));
                break;
            case PickupType.Custom:
                pickup.RelayChange(new SetPickupTypeRpcPacket(pickup.Id, pickup.Model));
                break;
        }
    }
}
