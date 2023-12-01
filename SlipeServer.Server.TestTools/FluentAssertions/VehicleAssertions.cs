using FluentAssertions.Execution;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.TestTools.FluentAssertions;

public class VehicleAssertions : ElementAssertionsBase<Vehicle>
{
    public VehicleAssertions(Vehicle vehicle) : base(vehicle)
    {
    }

    public override void BeEquivalentTo(Vehicle vehicle, string because = "", params object[] becauseArgs)
    {
        using var _ = new AssertionScope();

        base.BeEquivalentTo(vehicle, because, becauseArgs);
        AssertPropertyEquality(e => e.Model, vehicle.Model, "Model", because, becauseArgs);
        AssertPropertyEquality(e => e.Health, vehicle.Health, "Model", because, becauseArgs);
        AssertPropertyEquality(e => e.Colors.Primary, vehicle.Colors.Primary, "Colors.Primary", because, becauseArgs);
        AssertPropertyEquality(e => e.Colors.Secondary, vehicle.Colors.Secondary, "Colors.Secondary", because, becauseArgs);
        AssertPropertyEquality(e => e.Colors.Color3, vehicle.Colors.Color3, "Colors.Color3", because, becauseArgs);
        AssertPropertyEquality(e => e.Colors.Color4, vehicle.Colors.Color4, "Colors.Color4", because, becauseArgs);
        AssertPropertyEquality(e => e.PaintJob, vehicle.PaintJob, "PaintJob", because, becauseArgs);
        AssertPropertyEquality(e => e.Damage.Lights, vehicle.Damage.Lights, "Damage.Lights", because, becauseArgs);
        AssertPropertyEquality(e => e.Damage.Wheels, vehicle.Damage.Wheels, "Damage.Wheels", because, becauseArgs);
        AssertPropertyEquality(e => e.Damage.Panels, vehicle.Damage.Panels, "Damage.Panels", because, becauseArgs);
        AssertPropertyEquality(e => e.Damage.Doors, vehicle.Damage.Doors, "Damage.Doors", because, becauseArgs);
        AssertPropertyEquality(e => e.Variants, vehicle.Variants, "Variants", because, becauseArgs);
        AssertPropertyEquality(e => e.RespawnPosition, vehicle.RespawnPosition, "RespawnPosition", because, becauseArgs);
        AssertPropertyEquality(e => e.RespawnRotation, vehicle.RespawnRotation, "RespawnRotation", because, becauseArgs);
        AssertPropertyEquality(e => e.RespawnHealth, vehicle.RespawnHealth, "RespawnHealth", because, becauseArgs);
        AssertPropertyEquality(e => e.TurretRotation, vehicle.TurretRotation, "TurretRotation", because, becauseArgs);
        AssertPropertyEquality(e => e.AdjustableProperty, vehicle.AdjustableProperty, "AdjustableProperty", because, becauseArgs);
        AssertPropertyEquality(e => e.DoorRatios, vehicle.DoorRatios, "DoorRatios", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.Hood, vehicle.Upgrades.Hood, "Upgrades.Hood", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.Vent, vehicle.Upgrades.Vent, "Upgrades.Vent", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.Spoiler, vehicle.Upgrades.Spoiler, "Upgrades.Spoiler", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.Sideskirt, vehicle.Upgrades.Sideskirt, "Upgrades.Sideskirt", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.FrontBullbar, vehicle.Upgrades.FrontBullbar, "Upgrades.FrontBullbar", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.RearBullbar, vehicle.Upgrades.RearBullbar, "Upgrades.RearBullbar", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.Lamps, vehicle.Upgrades.Lamps, "Upgrades.Lamps", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.Roof, vehicle.Upgrades.Roof, "Upgrades.Roof", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.Nitro, vehicle.Upgrades.Nitro, "Upgrades.Nitro", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.HasHydraulics, vehicle.Upgrades.HasHydraulics, "Upgrades.HasHydraulics", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.HasStereo, vehicle.Upgrades.HasStereo, "Upgrades.HasStereo", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.Wheels, vehicle.Upgrades.Wheels, "Upgrades.Wheels", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.Exhaust, vehicle.Upgrades.Exhaust, "Upgrades.Exhaust", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.FrontBumper, vehicle.Upgrades.FrontBumper, "Upgrades.FrontBumper", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.RearBumper, vehicle.Upgrades.RearBumper, "Upgrades.RearBumper", because, becauseArgs);
        AssertPropertyEquality(e => e.Upgrades.Misc, vehicle.Upgrades.Misc, "Upgrades.Misc", because, becauseArgs);
        AssertPropertyEquality(e => e.PlateText, vehicle.PlateText, "PlateText", because, becauseArgs);
        AssertPropertyEquality(e => e.OverrideLights, vehicle.OverrideLights, "OverrideLights", because, becauseArgs);
        AssertPropertyEquality(e => e.IsLandingGearDown, vehicle.IsLandingGearDown, "IsLandingGearDown", because, becauseArgs);
        AssertPropertyEquality(e => e.IsEngineOn, vehicle.IsEngineOn, "IsEngineOn", because, becauseArgs);
        AssertPropertyEquality(e => e.IsLocked, vehicle.IsLocked, "IsLocked", because, becauseArgs);
        AssertPropertyEquality(e => e.AreDoorsDamageProof, vehicle.AreDoorsDamageProof, "AreDoorsDamageProof", because, becauseArgs);
        AssertPropertyEquality(e => e.IsDamageProof, vehicle.IsDamageProof, "IsDamageProof", because, becauseArgs);
        AssertPropertyEquality(e => e.IsDerailed, vehicle.IsDerailed, "IsDerailed", because, becauseArgs);
        AssertPropertyEquality(e => e.IsDerailable, vehicle.IsDerailable, "IsDerailable", because, becauseArgs);
        AssertPropertyEquality(e => e.TrainDirection, vehicle.TrainDirection, "TrainDirection", because, becauseArgs);
        AssertPropertyEquality(e => e.IsTaxiLightOn, vehicle.IsTaxiLightOn, "IsTaxiLightOn", because, becauseArgs);
        AssertPropertyEquality(e => e.HeadlightColor, vehicle.HeadlightColor, "HeadlightColor", because, becauseArgs);
        AssertPropertyEquality(e => e.Handling, vehicle.Handling, "Handling", because, becauseArgs);
        AssertPropertyEquality(e => e.Sirens, vehicle.Sirens, "Sirens", because, becauseArgs);
        AssertPropertyEquality(e => e.IsSirenActive, vehicle.IsSirenActive, "IsSirenActive", because, becauseArgs);
        AssertPropertyEquality(e => e.IsInWater, vehicle.IsInWater, "IsInWater", because, becauseArgs);
        AssertPropertyEquality(e => e.BlownState, vehicle.BlownState, "BlownState", because, becauseArgs);
    }
}
