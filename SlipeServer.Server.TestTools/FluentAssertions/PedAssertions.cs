using FluentAssertions.Execution;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.TestTools.FluentAssertions;

public class PedAssertions : ElementAssertionsBase<Ped>
{
    public PedAssertions(Ped ped) : base(ped)
    {
    }

    public override void BeEquivalentTo(Ped ped, string because = "", params object[] becauseArgs)
    {
        using var _ = new AssertionScope();

        base.BeEquivalentTo(ped, because, becauseArgs);
        AssertPropertyEquality(e => e.Model, ped.Model, "Model", because, becauseArgs);
        AssertPropertyEquality(e => e.Health, ped.Health, "Health", because, becauseArgs);
        AssertPropertyEquality(e => e.Armor, ped.Armor, "Armor", because, becauseArgs);
        AssertPropertyEquality(e => e.CurrentWeaponSlot, ped.CurrentWeaponSlot, "CurrentWeaponSlot", because, becauseArgs);
        AssertPropertyEquality(e => e.FightingStyle, ped.FightingStyle, "FightingStyle", because, becauseArgs);
        AssertPropertyEquality(e => e.Gravity, ped.Gravity, "Gravity", because, becauseArgs);
        AssertPropertyEquality(e => e.CurrentWeapon, ped.CurrentWeapon, "CurrentWeapon", because, becauseArgs);
        AssertPropertyEquality(e => e.EnteringVehicle, ped.EnteringVehicle, "EnteringVehicle", because, becauseArgs);
        AssertPropertyEquality(e => e.Vehicle, ped.Vehicle, "Vehicle", because, becauseArgs);
        AssertPropertyEquality(e => e.Seat, ped.Seat, "Seat", because, becauseArgs);
        AssertPropertyEquality(e => e.HasJetpack, ped.HasJetpack, "HasJetpack", because, becauseArgs);
        AssertPropertyEquality(e => e.IsSyncable, ped.IsSyncable, "IsSyncable", because, becauseArgs);
        AssertPropertyEquality(e => e.IsHeadless, ped.IsHeadless, "IsHeadless", because, becauseArgs);
        AssertPropertyEquality(e => e.MoveAnimation, ped.MoveAnimation, "MoveAnimation", because, becauseArgs);
        AssertPropertyEquality(e => e.IsOnFire, ped.IsOnFire, "IsOnFire", because, becauseArgs);
        AssertPropertyEquality(e => e.IsInWater, ped.IsInWater, "IsInWater", because, becauseArgs);
        AssertPropertyEquality(e => e.VehicleAction, ped.VehicleAction, "VehicleAction", because, becauseArgs);
    }
}
