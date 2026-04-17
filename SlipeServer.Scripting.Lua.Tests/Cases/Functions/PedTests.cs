using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Packets.Enums;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class PedTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void CreatePed_CreatesPed(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Ped> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("createPed(7, 1, 2, 3)");

        captures.Should().ContainSingle();
        captures[0].Model.Should().Be(7);
        captures[0].Position.Should().Be(new Vector3(1, 2, 3));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPedGravity_ReturnsDefaultGravity(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(getPedGravity(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Contain("0.008");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedGravity_ChangesGravity(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("setPedGravity(testPed, 0.02)");

        ped.Gravity.Should().BeApproximately(0.02f, 0.0001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPedArmor_ReturnsZeroByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(getPedArmor(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedArmor_ChangesArmor(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("setPedArmor(testPed, 50)");

        ped.Armor.Should().Be(50);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPedDead_ReturnsFalseWhenAlive(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(isPedDead(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void KillPed_KillsPed(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            killPed(testPed)
            assertPrint(tostring(isPedDead(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        ped.IsAlive.Should().BeFalse();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPedHeadless_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(isPedHeadless(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedHeadless_MakesPedHeadless(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("setPedHeadless(testPed, true)");

        ped.IsHeadless.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPedWearingJetpack_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(isPedWearingJetpack(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedWearingJetpack_EquipsJetpack(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("setPedWearingJetpack(testPed, true)");

        ped.HasJetpack.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPedChoking_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(isPedChoking(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedChoking_SetsChokingState(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("setPedChoking(testPed, true)");

        ped.IsChoking.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPedDoingGangDriveby_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(isPedDoingGangDriveby(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedDoingGangDriveby_SetsGangDrivebyState(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("setPedDoingGangDriveby(testPed, true)");

        ped.IsDoingGangDriveby.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPedInVehicle_ReturnsFalseWhenNotInVehicle(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(isPedInVehicle(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void WarpPedIntoVehicle_PedIsInVehicle(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        var vehicle = new Vehicle(411, new Vector3(5, 0, 0)).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            warpPedIntoVehicle(testPed, testVehicle)
            assertPrint(tostring(isPedInVehicle(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        ped.Vehicle.Should().Be(vehicle);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RemovePedFromVehicle_RemovesPedFromVehicle(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        var vehicle = new Vehicle(411, new Vector3(5, 0, 0)).AssociateWith(sut);
        ped.WarpIntoVehicle(vehicle);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("removePedFromVehicle(testPed)");

        ped.Vehicle.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPedFightingStyle_ReturnsDefaultFightingStyle(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(getPedFightingStyle(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("4");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedFightingStyle_ChangesFightingStyle(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("setPedFightingStyle(testPed, 5)");

        ped.FightingStyle.Should().Be(FightingStyle.Boxing);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPedStat_ReturnsZeroByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(getPedStat(testPed, 22)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedStat_ChangesStat(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            setPedStat(testPed, 22, 500)
            assertPrint(tostring(getPedStat(testPed, 22)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("500");
        ped.GetStat(PedStat.STAMINA).Should().Be(500f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetValidPedModels_ReturnsNonEmptyTable(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local models = getValidPedModels()
            assertPrint(tostring(#models > 0))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedAnimation_DoesNotThrow(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        PedAnimationStartedEventArgs? capturedArgs = null;
        ped.AnimationStarted += (_, args) => capturedArgs = args;

        var act = () => sut.RunLuaScript("""
            setPedAnimation(testPed, "ped", "walk_start", 1000, false)
            """);

        act.Should().NotThrow();
        capturedArgs.Should().NotBeNull();
        capturedArgs!.Block.Should().Be("ped");
        capturedArgs.Animation.Should().Be("walk_start");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedAnimation_WithNil_StopsAnimation(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        bool animationStopped = false;
        ped.AnimationStopped += (_, _) => animationStopped = true;

        var act = () => sut.RunLuaScript("""
            setPedAnimation(testPed)
            """);

        act.Should().NotThrow();
        animationStopped.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedAnimationProgress_DoesNotThrow(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        bool progressChanged = false;
        ped.AnimationProgressChanged += (_, _) => progressChanged = true;

        var act = () => sut.RunLuaScript("""
            setPedAnimationProgress(testPed, "walk_start", 0.5)
            """);

        act.Should().NotThrow();
        progressChanged.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedAnimationSpeed_DoesNotThrow(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        bool speedChanged = false;
        ped.AnimationSpeedChanged += (_, _) => speedChanged = true;

        var act = () => sut.RunLuaScript("""
            setPedAnimationSpeed(testPed, "walk_start", 1.5)
            """);

        act.Should().NotThrow();
        speedChanged.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ReloadPedWeapon_DoesNotThrow(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        bool reloadFired = false;
        ped.WeaponReloaded += (_, _) => reloadFired = true;

        var act = () => sut.RunLuaScript("""
            reloadPedWeapon(testPed)
            """);

        act.Should().NotThrow();
        reloadFired.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AddPedClothes_GetPedClothes_ReturnsCorrectClothing(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            addPedClothes(testPed, "vest", "vest", 0)
            local texture, model = getPedClothes(testPed, 0)
            assertPrint(texture .. "," .. model)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("vest,vest");
        ped.Clothing.GetClothing().Should().Contain(c => c.Texture == "vest" && c.Model == "vest");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedRotation_ChangesRotation(IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assert(setPedRotation(testPed, 180))
            """);

        ped.Rotation.Z.Should().BeApproximately(180f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPedRotation_ReturnsRotation(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero) { Rotation = new Vector3(0, 0, 90) }.AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(getPedRotation(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Contain("90");
        ped.Rotation.Z.Should().BeApproximately(90f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RemovePedClothes_DoesNotThrow(
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        var act = () => sut.RunLuaScript("""
            addPedClothes(testPed, "vest", "vest", 0)
            removePedClothes(testPed, 0)
            """);

        act.Should().NotThrow();
        ped.Clothing.GetClothing().Should().NotContain(c => c.Texture == "vest" && c.Model == "vest");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPedDucked_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(isPedDucked(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPedOnGround_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(isPedOnGround(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPedReloadingWeapon_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(isPedReloadingWeapon(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPedOccupiedVehicle_ReturnsNilWhenNotInVehicle(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(getPedOccupiedVehicle(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPedWalkingStyle_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            assertPrint(tostring(getPedWalkingStyle(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPedWalkingStyle_ChangesWalkingStyle(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            setPedWalkingStyle(testPed, 54)
            assertPrint(tostring(getPedWalkingStyle(testPed)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("54");
        ped.MoveAnimation.Should().Be(PedMoveAnimation.Player);
    }
}
