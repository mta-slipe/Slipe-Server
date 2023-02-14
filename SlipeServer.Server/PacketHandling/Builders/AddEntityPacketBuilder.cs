using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Enums.VehicleUpgrades;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Builders;

public class AddEntityPacketBuilder
{
    private readonly AddEntityPacket packet;

    public AddEntityPacketBuilder()
    {
        this.packet = new AddEntityPacket();
    }

    public void AddDummy(DummyElement element)
    {
        this.packet.AddDummy(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            element.Attachment, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.ElementTypeName, element.Position);
    }

    public void AddObject(WorldObject element)
    {
        this.packet.AddObject(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            element.Attachment, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.Position, element.Rotation, element.Model, element.Alpha, element.IsLowLod, element.LowLodElement?.Id,
            element.DoubleSided, element.IsVisibleInAllDimensions, element.Movement, element.Scale, element.IsFrozen, element.Health
        );
    }

    public void AddBlip(Blip element)
    {
        this.packet.AddBlip(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            element.Attachment, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.Position, element.Ordering, element.VisibleDistance, (byte)element.Icon, element.Size, element.Color
        );
    }

    public void AddColShape(CollisionShape element)
    {
        switch (element)
        {
            case CollisionCircle collisionCircle:
                this.packet.AddColCircle(collisionCircle.Id, (byte)collisionCircle.ElementType, collisionCircle.Parent?.Id ?? ElementId.Zero, collisionCircle.Interior, collisionCircle.Dimension,
                    element.Attachment, collisionCircle.AreCollisionsEnabled, collisionCircle.IsCallPropagationEnabled, element.BroadcastableElementData, collisionCircle.Name, collisionCircle.TimeContext,
                    (byte)ColShapeType.Circle, collisionCircle.Position, collisionCircle.IsEnabled, collisionCircle.AutoCallEvent, collisionCircle.Radius
                );
                break;

            case CollisionCuboid collisionCuboid:
                this.packet.AddColCuboid(collisionCuboid.Id, (byte)collisionCuboid.ElementType, collisionCuboid.Parent?.Id ?? ElementId.Zero, collisionCuboid.Interior,
                    collisionCuboid.Dimension, element.Attachment, collisionCuboid.AreCollisionsEnabled, collisionCuboid.IsCallPropagationEnabled, element.BroadcastableElementData,
                    collisionCuboid.Name, collisionCuboid.TimeContext, (byte)ColShapeType.Cuboid, collisionCuboid.Position, collisionCuboid.IsEnabled,
                    collisionCuboid.AutoCallEvent, collisionCuboid.Dimensions
                );
                break;

            case CollisionRectangle collisionRectangle:
                this.packet.AddColRectangle(collisionRectangle.Id, (byte)collisionRectangle.ElementType, collisionRectangle.Parent?.Id ?? ElementId.Zero, collisionRectangle.Interior,
                    collisionRectangle.Dimension, element.Attachment, collisionRectangle.AreCollisionsEnabled, collisionRectangle.IsCallPropagationEnabled, element.BroadcastableElementData,
                    collisionRectangle.Name, collisionRectangle.TimeContext, (byte)ColShapeType.Rectangle, collisionRectangle.Position, collisionRectangle.IsEnabled,
                    collisionRectangle.AutoCallEvent, collisionRectangle.Dimensions
                );
                break;

            case CollisionSphere collisionShpere:
                this.packet.AddColSphere(collisionShpere.Id, (byte)collisionShpere.ElementType, collisionShpere.Parent?.Id ?? ElementId.Zero, collisionShpere.Interior, collisionShpere.Dimension,
                    element.Attachment, collisionShpere.AreCollisionsEnabled, collisionShpere.IsCallPropagationEnabled, element.BroadcastableElementData, collisionShpere.Name, collisionShpere.TimeContext,
                    (byte)ColShapeType.Sphere, collisionShpere.Position, collisionShpere.IsEnabled, collisionShpere.AutoCallEvent, collisionShpere.Radius
                );
                break;

            case CollisionTube collisionTube:
                this.packet.AddColTube(collisionTube.Id, (byte)collisionTube.ElementType, collisionTube.Parent?.Id ?? ElementId.Zero, collisionTube.Interior, collisionTube.Dimension,
                    element.Attachment, collisionTube.AreCollisionsEnabled, collisionTube.IsCallPropagationEnabled, element.BroadcastableElementData, collisionTube.Name, collisionTube.TimeContext,
                    (byte)ColShapeType.Tube, collisionTube.Position, collisionTube.IsEnabled, collisionTube.AutoCallEvent, collisionTube.Radius, collisionTube.Height
                );
                break;

            case CollisionPolygon collisionPolygon:
                this.packet.AddColPolygon(collisionPolygon.Id, (byte)collisionPolygon.ElementType, collisionPolygon.Parent?.Id ?? ElementId.Zero, collisionPolygon.Interior, collisionPolygon.Dimension,
                    element.Attachment, collisionPolygon.AreCollisionsEnabled, collisionPolygon.IsCallPropagationEnabled, element.BroadcastableElementData, collisionPolygon.Name, collisionPolygon.TimeContext,
                    (byte)ColShapeType.Polygon, collisionPolygon.Position, collisionPolygon.IsEnabled, collisionPolygon.AutoCallEvent, collisionPolygon.GetVertices().ToArray(), collisionPolygon.Height
                );
                break;

        }
    }

    public void AddMarker(Marker element)
    {
        this.packet.AddMarker(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            element.Attachment, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.Position, (byte)element.MarkerType, element.Size, element.Color, element.TargetPosition
        );
    }

    public void AddPed(Ped element)
    {
        this.packet.AddPed(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            element.Attachment, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.Position, element.Model, element.PedRotation, element.Health, element.Armor, element.Vehicle?.Id, element.Seat,
            element.HasJetpack, element.IsSyncable, element.IsHeadless, element.IsFrozen, element.Alpha, (byte)element.MoveAnimation,
            element.Clothing.GetClothing().ToArray(), element.Weapons.Cast<PedWeapon>().ToArray(), (byte)(element.CurrentWeapon?.Slot ?? 0)
        );
    }

    public void AddPickup(Pickup element)
    {
        this.packet.AddPickup(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            element.Attachment, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.Position, element.Model, element.IsVisible, (byte)element.PickupType, element.Armor, element.Health, (byte?)element.WeaponType, element.Ammo
        );
    }

    public void AddRadarArea(RadarArea element)
    {
        this.packet.AddRadarArea(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            element.Attachment, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.Position2, element.Size, element.Color, element.IsFlashing
        );
    }

    public void AddTeam(Team element)
    {
        this.packet.AddTeam(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            element.Attachment, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.TeamName, element.Color, element.IsFriendlyFireEnabled, element.Players.Select(p => p.Id).ToArray()
        );
    }

    public void AddWater(Water element)
    {
        this.packet.AddWater(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            element.Attachment, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.Vertices.ToArray(), element.IsShallow
        );
    }

    public void AddVehicle(Vehicle element)
    {
        this.packet.AddVehicle(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            element.Attachment, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.Position, element.Rotation, element.Model, element.Health, (byte)element.BlownState, element.Colors.AsArray(), element.PaintJob, element.Damage, element.Variants.Variant1,
            element.Variants.Variant2, element.TurretRotation, element.AdjustableProperty, VehicleConstants.DoorsPerVehicle[(VehicleModel)element.Model] > 0 ? element.DoorRatios.ToArray() : Array.Empty<float>(), MapVehicleUpgrades(element.Model, element.Upgrades), element.PlateText, 
            (byte)element.OverrideLights, element.IsLandingGearDown, element.IsSirenActive, element.IsFuelTankExplodable, element.IsEngineOn, element.IsLocked, 
            element.AreDoorsDamageProof, element.IsDamageProof, element.IsFrozen, element.IsDerailed, element.IsDerailable, element.TrainDirection == Elements.Enums.TrainDirection.Clockwise, element.IsTaxiLightOn, 
            element.Alpha, element.HeadlightColor, element.Handling, element.Sirens
        );
    }

    private byte[] MapVehicleUpgrades(ushort model, VehicleUpgrades upgrades)
    {
        var upgradeList = new List<byte>();

        if (upgrades.Hood != VehicleUpgradeHood.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeHood>(model, (ushort)upgrades.Hood) - 1000)!);

        if (upgrades.Vent != VehicleUpgradeVent.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeVent>(model, (ushort)upgrades.Vent) - 1000)!);

        if (upgrades.Spoiler != VehicleUpgradeSpoiler.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeSpoiler>(model, (ushort)upgrades.Spoiler) - 1000)!);

        if (upgrades.Sideskirt != VehicleUpgradeSideskirt.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeSideskirt>(model, (ushort)upgrades.Sideskirt) - 1000)!);

        if (upgrades.FrontBullbar != VehicleUpgradeFrontBullbar.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeFrontBullbar>(model, (ushort)upgrades.FrontBullbar) - 1000)!);

        if (upgrades.RearBullbar != VehicleUpgradeRearBullbar.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeRearBullbar>(model, (ushort)upgrades.RearBullbar) - 1000)!);

        if (upgrades.Lamps != VehicleUpgradeLamp.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeLamp>(model, (ushort)upgrades.Lamps) - 1000)!);

        if (upgrades.Roof != VehicleUpgradeRoof.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeRoof>(model, (ushort)upgrades.Roof) - 1000)!);

        if (upgrades.Nitro != VehicleUpgradeNitro.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeNitro>(model, (ushort)upgrades.Nitro) - 1000)!);

        if (upgrades.Wheels != VehicleUpgradeWheel.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeWheel>(model, (ushort)upgrades.Wheels) - 1000)!);

        if (upgrades.Exhaust != VehicleUpgradeExhaust.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeExhaust>(model, (ushort)upgrades.Exhaust) - 1000)!);

        if (upgrades.FrontBumper != VehicleUpgradeFrontBumper.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeFrontBumper>(model, (ushort)upgrades.FrontBumper) - 1000)!);

        if (upgrades.RearBumper != VehicleUpgradeRearBumper.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeRearBumper>(model, (ushort)upgrades.RearBumper) - 1000)!);

        if (upgrades.HasHydraulics)
            upgradeList.Add((byte)(VehicleUpgradeConstants.HydraulicsId - 1000));

        if (upgrades.HasStereo)
            upgradeList.Add((byte)(VehicleUpgradeConstants.StereoId - 1000));

        if (upgrades.Misc != VehicleUpgradeMisc.None)
            upgradeList.Add((byte)(VehicleUpgradeConstants.GetUpgradeIdForVehicle<VehicleUpgradeMisc>(model, (ushort)upgrades.Misc) - 1000)!);

        return upgradeList.ToArray();
    }

    public void AddWeapon(WeaponObject element)
    {
        this.packet.AddWeapon(element.Id, (byte)element.ElementType, element.Parent?.Id ?? ElementId.Zero, element.Interior, element.Dimension,
            null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, element.BroadcastableElementData, element.Name, element.TimeContext,
            element.Position, element.Rotation, element.Model, element.Alpha, element.IsLowLod, element.LowLodElement?.Id,
            element.DoubleSided, element.IsVisibleInAllDimensions, element.Movement, element.Scale, element.IsFrozen, element.Health,
            (byte)element.TargetType, element.TargetElement?.Id, element.BoneTarget, element.WheelTarget, element.TargetPosition, element.IsChanged,
            element.DamagePerHit, element.Accuracy, element.TargetRange, element.WeaponRange, element.DisableWeaponModel, element.InstantReload,
            element.ShootIfTargetBlocked, element.ShootIfTargetOutOfRange, element.CheckBuildings, element.CheckCarTires, element.CheckDummies,
            element.CheckObjects, element.CheckPeds, element.CheckVehicles, element.IgnoreSomeObjectForCamera, element.SeeThroughStuff,
            element.ShootThroughStuff, (byte)element.WeaponState, element.Ammo, element.ClipAmmo, element.Owner?.Id ?? (ElementId)PacketConstants.InvalidElementId
        );
    }



    public AddEntityPacket Build()
    {
        return this.packet;
    }
}
