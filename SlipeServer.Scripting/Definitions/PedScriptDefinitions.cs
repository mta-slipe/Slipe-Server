using SlipeServer.Packets.Enums;
using SlipeServer.Server;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class PedScriptDefinitions(IMtaServer server)
{
    [ScriptFunctionDefinition("createPed")]
    public Ped CreatePed(int modelid, float x, float y, float z, float rot = 0.0f, bool synced = true)
    {
        var ped = new Ped((PedModel)modelid, new Vector3(x, y, z))
        {
            Rotation = new Vector3(0, 0, rot),
            IsSyncable = synced
        }.AssociateWith(server);

        if (ScriptExecutionContext.Current?.Owner != null)
            ped.Parent = ScriptExecutionContext.Current.Owner?.DynamicRoot;

        return ped;
    }

    [ScriptFunctionDefinition("getPedGravity")]
    public float GetPedGravity(Ped ped) => ped.Gravity;

    [ScriptFunctionDefinition("setPedGravity")]
    public bool SetPedGravity(Ped ped, float gravity)
    {
        ped.Gravity = gravity;
        return true;
    }

    [ScriptFunctionDefinition("getPedArmor")]
    public float GetPedArmor(Ped ped) => ped.Armor;

    [ScriptFunctionDefinition("setPedArmor")]
    public bool SetPedArmor(Ped ped, float armor)
    {
        ped.Armor = armor;
        return true;
    }

    [ScriptFunctionDefinition("getPedWeapon")]
    public int GetPedWeapon(Ped ped, int? weaponSlot = null)
    {
        var slot = weaponSlot.HasValue ? (WeaponSlot)weaponSlot.Value : ped.CurrentWeaponSlot;
        var weapon = ped.Weapons.Get(slot);
        return (int)(weapon?.Type ?? 0);
    }

    [ScriptFunctionDefinition("getPedWeaponSlot")]
    public int GetPedWeaponSlot(Ped ped) => (int)ped.CurrentWeaponSlot;

    [ScriptFunctionDefinition("setPedWeaponSlot")]
    public bool SetPedWeaponSlot(Ped ped, int weaponSlot)
    {
        ped.CurrentWeaponSlot = (WeaponSlot)weaponSlot;
        return true;
    }

    [ScriptFunctionDefinition("getPedAmmoInClip")]
    public int GetPedAmmoInClip(Ped ped, int? weaponSlot = null)
    {
        var slot = weaponSlot.HasValue ? (WeaponSlot)weaponSlot.Value : ped.CurrentWeaponSlot;
        var weapon = ped.Weapons.Get(slot);
        return weapon?.AmmoInClip ?? 0;
    }

    [ScriptFunctionDefinition("getPedTotalAmmo")]
    public int GetPedTotalAmmo(Ped ped, int? weaponSlot = null)
    {
        var slot = weaponSlot.HasValue ? (WeaponSlot)weaponSlot.Value : ped.CurrentWeaponSlot;
        var weapon = ped.Weapons.Get(slot);
        return weapon?.Ammo ?? 0;
    }

    [ScriptFunctionDefinition("getPedFightingStyle")]
    public int GetPedFightingStyle(Ped ped) => (int)ped.FightingStyle;

    [ScriptFunctionDefinition("setPedFightingStyle")]
    public bool SetPedFightingStyle(Ped ped, int style)
    {
        ped.FightingStyle = (FightingStyle)style;
        return true;
    }

    [ScriptFunctionDefinition("getPedWalkingStyle")]
    public int GetPedWalkingStyle(Ped ped) => (int)ped.MoveAnimation;

    [ScriptFunctionDefinition("setPedWalkingStyle")]
    public bool SetPedWalkingStyle(Ped ped, int style)
    {
        ped.MoveAnimation = (PedMoveAnimation)style;
        return true;
    }

    [ScriptFunctionDefinition("getPedOccupiedVehicle")]
    public Vehicle? GetPedOccupiedVehicle(Ped ped) => ped.Vehicle;

    [ScriptFunctionDefinition("getPedOccupiedVehicleSeat")]
    public int? GetPedOccupiedVehicleSeat(Ped ped) => ped.Seat;

    [ScriptFunctionDefinition("isPedInVehicle")]
    public bool IsPedInVehicle(Ped ped) => ped.Vehicle != null;

    [ScriptFunctionDefinition("isPedDead")]
    public bool IsPedDead(Ped ped) => !ped.IsAlive;

    [ScriptFunctionDefinition("isPedHeadless")]
    public bool IsPedHeadless(Ped ped) => ped.IsHeadless;

    [ScriptFunctionDefinition("setPedHeadless")]
    public bool SetPedHeadless(Ped ped, bool headless)
    {
        ped.IsHeadless = headless;
        return true;
    }

    [ScriptFunctionDefinition("isPedWearingJetpack")]
    public bool IsPedWearingJetpack(Ped ped) => ped.HasJetpack;

    [ScriptFunctionDefinition("setPedWearingJetpack")]
    public bool SetPedWearingJetpack(Ped ped, bool state)
    {
        ped.HasJetpack = state;
        return true;
    }

    [ScriptFunctionDefinition("isPedChoking")]
    public bool IsPedChoking(Ped ped) => ped.IsChoking;

    [ScriptFunctionDefinition("setPedChoking")]
    public bool SetPedChoking(Ped ped, bool choking)
    {
        ped.IsChoking = choking;
        return true;
    }

    [ScriptFunctionDefinition("isPedDoingGangDriveby")]
    public bool IsPedDoingGangDriveby(Ped ped) => ped.IsDoingGangDriveby;

    [ScriptFunctionDefinition("setPedDoingGangDriveby")]
    public bool SetPedDoingGangDriveby(Ped ped, bool state)
    {
        ped.IsDoingGangDriveby = state;
        return true;
    }

    [ScriptFunctionDefinition("isPedDucked")]
    public bool IsPedDucked(Ped ped) => ped is Player player && player.IsDucked;

    [ScriptFunctionDefinition("isPedOnGround")]
    public bool IsPedOnGround(Ped ped) => ped is Player player && player.IsOnGround;

    [ScriptFunctionDefinition("isPedReloadingWeapon")]
    public bool IsPedReloadingWeapon(Ped ped) => false;

    [ScriptFunctionDefinition("reloadPedWeapon")]
    public bool ReloadPedWeapon(Ped ped)
    {
        ped.ReloadWeapon();
        return true;
    }

    [ScriptFunctionDefinition("killPed")]
    public bool KillPed(Ped ped, Ped? theKiller = null, int weapon = 255, int bodyPart = 255, bool stealth = false)
    {
        var damageType = (DamageType)weapon;
        var bodyPartEnum = bodyPart is >= 3 and <= 9 ? (BodyPart)bodyPart : BodyPart.Torso;
        ped.Kill(theKiller, damageType, bodyPartEnum);
        return true;
    }

    [ScriptFunctionDefinition("getPedTarget")]
    public Element? GetPedTarget(Ped ped) => ped.Target;

    [ScriptFunctionDefinition("getPedStat")]
    public float GetPedStat(Ped ped, int stat) => ped.GetStat((PedStat)stat);

    [ScriptFunctionDefinition("setPedStat")]
    public bool SetPedStat(Ped ped, int stat, float value)
    {
        ped.SetStat((PedStat)stat, value);
        return true;
    }

    [ScriptFunctionDefinition("getPedContactElement")]
    public Element? GetPedContactElement(Ped ped) => ped is Player player ? player.ContactElement : null;

    [ScriptFunctionDefinition("getValidPedModels")]
    public IEnumerable<int> GetValidPedModels()
    {
        return System.Enum.GetValues<PedModel>().Select(m => (int)m);
    }

    [ScriptFunctionDefinition("removePedFromVehicle")]
    public bool RemovePedFromVehicle(Ped ped)
    {
        ped.RemoveFromVehicle();
        return true;
    }

    [ScriptFunctionDefinition("warpPedIntoVehicle")]
    public bool WarpPedIntoVehicle(Ped ped, Vehicle theVehicle, int seat = 0)
    {
        ped.WarpIntoVehicle(theVehicle, (byte)seat);
        return true;
    }

    [ScriptFunctionDefinition("setPedAnimation")]
    public bool SetPedAnimation(
        Ped ped,
        string? block = null,
        string? anim = null,
        int time = -1,
        bool loop = true,
        bool updatePosition = true,
        bool interruptable = true,
        bool freezeLastFrame = true,
        int blendTime = 250,
        bool retainPedState = false)
    {
        if (block == null || anim == null)
        {
            ped.StopAnimation();
            return true;
        }

        ped.SetAnimation(
            block,
            anim,
            System.TimeSpan.FromMilliseconds(time),
            loop,
            updatePosition,
            interruptable,
            freezeLastFrame,
            System.TimeSpan.FromMilliseconds(blendTime),
            retainPedState);

        return true;
    }

    [ScriptFunctionDefinition("setPedAnimationProgress")]
    public bool SetPedAnimationProgress(Ped ped, string anim, float progress)
    {
        ped.SetAnimationProgress(anim, progress);
        return true;
    }

    [ScriptFunctionDefinition("setPedAnimationSpeed")]
    public bool SetPedAnimationSpeed(Ped ped, string anim, float speed)
    {
        ped.SetAnimationSpeed(anim, speed);
        return true;
    }

    [ScriptFunctionDefinition("getPedRotation")]
    public float GetPedRotation(Ped ped) => ped.Rotation.Z;

    [ScriptFunctionDefinition("setPedRotation")]
    public bool SetPedRotation(Ped ped, float rotation, bool conformPedRotation = false)
    {
        ped.Rotation = new Vector3(ped.Rotation.X, ped.Rotation.Y, rotation);
        return true;
    }

    [ScriptFunctionDefinition("addPedClothes")]
    public bool AddPedClothes(Ped ped, string clothesTexture, string clothesModel, int clothesType)
    {
        ped.Clothing.AddClothing(new SlipeServer.Packets.Definitions.Entities.Structs.PedClothing(clothesTexture, clothesModel, (byte)clothesType));
        return true;
    }

    [ScriptFunctionDefinition("getPedClothes")]
    public PedClothingInfo GetPedClothes(Ped ped, int clothesType)
    {
        var type = (ClothingType)clothesType;
        if (!ClothingConstants.ClothesTextureModel.ContainsKey(type))
            return new PedClothingInfo("", "");

        byte? index = GetClothingIndex(ped.Clothing, type);
        if (index == null)
            return new PedClothingInfo("", "");

        var clothingData = ClothingConstants.ClothesTextureModel[type];
        if (index.Value >= clothingData.Length)
            return new PedClothingInfo("", "");

        var clothing = clothingData[index.Value];
        return new PedClothingInfo(clothing.Texture, clothing.Model);
    }

    [ScriptFunctionDefinition("removePedClothes")]
    public bool RemovePedClothes(Ped ped, int clothesType, string? clothesTexture = null, string? clothesModel = null)
    {
        ped.Clothing.RemoveClothing((ClothingType)clothesType);
        return true;
    }

    private static byte? GetClothingIndex(Server.ElementConcepts.Clothing clothing, ClothingType type) => type switch
    {
        ClothingType.Shirt => clothing.Shirt,
        ClothingType.Head => clothing.Head,
        ClothingType.Trousers => clothing.Trousers,
        ClothingType.Shoes => clothing.Shoes,
        ClothingType.TattoosLeftUpperArm => clothing.TattoosLeftUpperArm,
        ClothingType.TattoosLeftLowerArm => clothing.TattoosLeftLowerArm,
        ClothingType.TattoosRightUpperArm => clothing.TattoosRightUpperArm,
        ClothingType.TattoosRightLowerArm => clothing.TattoosRightLowerArm,
        ClothingType.TattoosBack => clothing.TattoosBack,
        ClothingType.TattoosLeftChest => clothing.TattoosLeftChest,
        ClothingType.TattoosRightChest => clothing.TattoosRightChest,
        ClothingType.TattoosStomach => clothing.TattoosStomach,
        ClothingType.TattoosLowerBack => clothing.TattoosLowerBack,
        ClothingType.Necklace => clothing.Necklace,
        ClothingType.Watches => clothing.Watch,
        ClothingType.Glasses => clothing.Glasses,
        ClothingType.Hats => clothing.Hat,
        ClothingType.Extra => clothing.Extra,
        _ => null
    };
}
