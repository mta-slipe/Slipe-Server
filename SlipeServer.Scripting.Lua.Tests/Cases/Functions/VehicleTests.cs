using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Packets.Enums;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class VehicleTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void CreateVehicle_CreatesVehicle(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Vehicle> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("createVehicle(411, 1, 2, 3)");

        captures.Should().ContainSingle();
        captures[0].Model.Should().Be(411);
        captures[0].Position.Should().Be(new Vector3(1, 2, 3));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateVehicle_WithPlate_SetsPlate(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Vehicle> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""createVehicle(411, 0, 0, 0, 0, 0, 0, "SLIPE01")""");

        captures.Should().ContainSingle();
        captures[0].PlateText.Should().Be("SLIPE01");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BlowVehicle_BlowsVehicle(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("blowVehicle(testVehicle)");

        vehicle.BlownState.Should().Be(VehicleBlownState.BlownUp);
        vehicle.Health.Should().Be(0);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FixVehicle_FixesVehicle(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        vehicle.BlowUp();
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assert(isVehicleBlown(testVehicle) == true)
            fixVehicle(testVehicle)
            assert(isVehicleBlown(testVehicle) == false)
            """);

        vehicle.BlownState.Should().Be(VehicleBlownState.Intact);
        vehicle.Health.Should().Be(1000);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SpawnVehicle_MovesVehicle(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("spawnVehicle(testVehicle, 10, 20, 30)");

        vehicle.Position.Should().Be(new Vector3(10, 20, 30));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetVehicleName_ReturnsCorrectName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assertPrint(getVehicleName(testVehicle))
            assertPrint(getVehicleNameFromModel(411))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("Infernus");
        assertDataProvider.AssertPrints[1].Should().Be("Infernus");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetVehicleModelFromName_ReturnsCorrectModel(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getVehicleModelFromName("Infernus")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("411");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetVehicleType_ReturnsCorrectType(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assertPrint(getVehicleType(testVehicle))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("Automobile");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleColor_Works(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehicleColor(testVehicle, 255, 0, 0, 0, 255, 0)
            local r, g, b, r2, g2, b2 = getVehicleColor(testVehicle, true)
            assertPrint(tostring(r))
            assertPrint(tostring(g))
            assertPrint(tostring(b))
            assertPrint(tostring(r2))
            assertPrint(tostring(g2))
            assertPrint(tostring(b2))
            """);

        vehicle.Colors.Primary.Should().Be(Color.FromArgb(255, 0, 0));
        vehicle.Colors.Secondary.Should().Be(Color.FromArgb(0, 255, 0));
        assertDataProvider.AssertPrints[0].Should().Be("255");
        assertDataProvider.AssertPrints[1].Should().Be("0");
        assertDataProvider.AssertPrints[2].Should().Be("0");
        assertDataProvider.AssertPrints[3].Should().Be("0");
        assertDataProvider.AssertPrints[4].Should().Be("255");
        assertDataProvider.AssertPrints[5].Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehiclePlateText_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehiclePlateText(testVehicle, "HELLO")
            assert(getVehiclePlateText(testVehicle) == "HELLO")
            """);

        vehicle.PlateText.Should().Be("HELLO");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleEngineState_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assert(getVehicleEngineState(testVehicle) == false)
            setVehicleEngineState(testVehicle, true)
            assert(getVehicleEngineState(testVehicle) == true)
            """);

        vehicle.IsEngineOn.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleLocked_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assert(isVehicleLocked(testVehicle) == false)
            setVehicleLocked(testVehicle, true)
            assert(isVehicleLocked(testVehicle) == true)
            """);

        vehicle.IsLocked.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleDoorState_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehicleDoorState(testVehicle, 0, 4)
            assert(getVehicleDoorState(testVehicle, 0) == 4)
            """);

        vehicle.GetDoorState(VehicleDoor.Hood).Should().Be(VehicleDoorState.Missing);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleWheelStates_Works(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehicleWheelStates(testVehicle, 2, 2, 2, 2)
            local fl, rl, fr, rr = getVehicleWheelStates(testVehicle)
            assertPrint(tostring(fl))
            assertPrint(tostring(rl))
            assertPrint(tostring(fr))
            assertPrint(tostring(rr))
            """);

        vehicle.GetWheelState(VehicleWheel.FrontLeft).Should().Be(VehicleWheelState.FallenOff);
        assertDataProvider.AssertPrints[0].Should().Be("2");
        assertDataProvider.AssertPrints[1].Should().Be("2");
        assertDataProvider.AssertPrints[2].Should().Be("2");
        assertDataProvider.AssertPrints[3].Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleDoorOpenRatio_Works(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehicleDoorOpenRatio(testVehicle, 0, 0.5)
            assertPrint(tostring(getVehicleDoorOpenRatio(testVehicle, 0)))
            """);

        vehicle.GetDoorOpenRatio(VehicleDoor.Hood).Should().BeApproximately(0.5f, 0.001f);
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().StartWith("0.5");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehiclePaintjob_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehiclePaintjob(testVehicle, 1)
            assert(getVehiclePaintjob(testVehicle) == 1)
            """);

        vehicle.PaintJob.Should().Be(1);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleVariant_Works(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehicleVariant(testVehicle, 1, 2)
            local v1, v2 = getVehicleVariant(testVehicle)
            assertPrint(tostring(v1))
            assertPrint(tostring(v2))
            """);

        vehicle.Variants.Variant1.Should().Be(1);
        vehicle.Variants.Variant2.Should().Be(2);
        assertDataProvider.AssertPrints[0].Should().Be("1");
        assertDataProvider.AssertPrints[1].Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AddRemoveVehicleUpgrade_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            addVehicleUpgrade(testVehicle, 1009)
            assert(getVehicleUpgradeOnSlot(testVehicle, 8) == 1009)
            removeVehicleUpgrade(testVehicle, 1009)
            assert(getVehicleUpgradeOnSlot(testVehicle, 8) == 0)
            """);

        vehicle.Upgrades.Nitro.Should().Be(SlipeServer.Packets.Enums.VehicleUpgrades.VehicleUpgradeNitro.None);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetVehicleCompatibleUpgrades_ReturnsUpgrades(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            local upgrades = getVehicleCompatibleUpgrades(testVehicle)
            assertPrint(tostring(#upgrades > 0))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetVehicleUpgradeSlotName_ReturnsCorrectName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(getVehicleUpgradeSlotName(8))
            assertPrint(getVehicleUpgradeSlotName(1009))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("nitro");
        assertDataProvider.AssertPrints[1].Should().Be("nitro");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AddRemoveVehicleSirens_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            addVehicleSirens(testVehicle, 2, 3)
            assert(getVehicleSirensOn(testVehicle) == false)
            setVehicleSirensOn(testVehicle, true)
            assert(getVehicleSirensOn(testVehicle) == true)
            removeVehicleSirens(testVehicle)
            """);

        vehicle.IsSirenActive.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetVehicleHandling_ReturnsTable(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            local h = getVehicleHandling(testVehicle)
            assertPrint(tostring(h ~= nil))
            assertPrint(tostring(h.mass ~= nil))
            assertPrint(tostring(h.driveType))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("true");
        assertDataProvider.AssertPrints[2].Should().NotBeEmpty();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetVehicleHandling_ModifiesVehicleHandling(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehicleHandling(testVehicle, "mass", 9999)
            local h = getVehicleHandling(testVehicle)
            assertPrint(tostring(h.mass))
            """);

        vehicle.Handling.Should().NotBeNull();
        vehicle.AppliedHandling.Mass.Should().BeApproximately(9999f, 1f);
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().StartWith("9999");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetModelHandling_ReturnsHandlingForModel(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local h = getModelHandling(411)
            assertPrint(tostring(h ~= nil))
            assertPrint(tostring(h.mass))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().NotBeEmpty();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetTrainDerailed_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Freight, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assert(isTrainDerailed(testVehicle) == false)
            setTrainDerailed(testVehicle, true)
            assert(isTrainDerailed(testVehicle) == true)
            """);

        vehicle.IsDerailed.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetTrainDerailable_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Freight, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assert(isTrainDerailable(testVehicle) == true)
            setTrainDerailable(testVehicle, false)
            assert(isTrainDerailable(testVehicle) == false)
            """);

        vehicle.IsDerailable.Should().BeFalse();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetTrainDirection_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Freight, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assert(getTrainDirection(testVehicle) == true)
            setTrainDirection(testVehicle, false)
            assert(getTrainDirection(testVehicle) == false)
            """);

        vehicle.TrainDirection.Should().Be(TrainDirection.CounterClockwise);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetTrainSpeed_Works(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Freight, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setTrainSpeed(testVehicle, 0.5)
            assertPrint(tostring(getTrainSpeed(testVehicle)))
            """);

        vehicle.TrainSpeed.Should().BeApproximately(0.5f, 0.001f);
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().StartWith("0.5");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleRespawnDelay_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehicleRespawnDelay(testVehicle, 5000)
            assert(getVehicleRespawnDelay(testVehicle) == 5000)
            """);

        vehicle.RespawnDelay.Should().Be(5000);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleRespawnPosition_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, new Vector3(1, 2, 3)).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehicleRespawnPosition(testVehicle, 10, 20, 30)
            assert(getVehicleRespawnPosition(testVehicle) ~= nil)
            """);

        vehicle.RespawnPosition.Should().Be(new Vector3(10, 20, 30));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ToggleVehicleRespawn_SetsRespawnable(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assert(isVehicleRespawnable(testVehicle) == false)
            toggleVehicleRespawn(testVehicle, true)
            assert(isVehicleRespawnable(testVehicle) == true)
            """);

        vehicle.IsRespawnable.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AttachDetachTrailer_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Roadtrain, Vector3.Zero).AssociateWith(sut);
        var trailer = new Vehicle(VehicleModel.Trailer1, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);
        sut.AddGlobal("testTrailer", trailer);

        sut.RunLuaScript("""
            attachTrailerToVehicle(testVehicle, testTrailer)
            assert(getVehicleTowedByVehicle(testVehicle) ~= nil)
            detachTrailerFromVehicle(testVehicle)
            assert(getVehicleTowedByVehicle(testVehicle) == nil)
            """);

        vehicle.TowedVehicle.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetVehicleMaxPassengers_ReturnsCorrectNumber(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assertPrint(tostring(getVehicleMaxPassengers(testVehicle)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().NotBe("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleHeadlightColor_Works(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            setVehicleHeadLightColor(testVehicle, 255, 0, 0)
            local r, g, b, a = getVehicleHeadLightColor(testVehicle)
            assertPrint(tostring(r))
            """);

        vehicle.HeadlightColor.R.Should().Be(255);
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("255");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleTaxiLight_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Taxi, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assert(isVehicleTaxiLightOn(testVehicle) == false)
            setVehicleTaxiLightOn(testVehicle, true)
            assert(isVehicleTaxiLightOn(testVehicle) == true)
            """);

        vehicle.IsTaxiLightOn.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSetVehicleDamageProof_Works(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            assert(isVehicleDamageProof(testVehicle) == false)
            setVehicleDamageProof(testVehicle, true)
            assert(isVehicleDamageProof(testVehicle) == true)
            """);

        vehicle.IsDamageProof.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RespawnVehicle_ResetsVehicle(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Infernus, new Vector3(0, 0, 0)).AssociateWith(sut);
        vehicle.Position = new Vector3(100, 200, 300);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("respawnVehicle(testVehicle)");

        vehicle.Position.Should().Be(new Vector3(0, 0, 0));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsVehicleOnGroundReturnsTrue(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Taxi, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        vehicle.IsOnGround = true;

        sut.RunLuaScript("""
            assert(isVehicleOnGround(testVehicle) == true)
            """);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsVehicleOnGroundReturnsFalse(IMtaServer sut)
    {
        var vehicle = new Vehicle(VehicleModel.Taxi, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testVehicle", vehicle);

        vehicle.IsOnGround = false;

        sut.RunLuaScript("""
            assert(isVehicleOnGround(testVehicle) == false)
            """);
    }
}
