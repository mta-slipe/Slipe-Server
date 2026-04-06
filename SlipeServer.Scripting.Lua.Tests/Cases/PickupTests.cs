using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class PickupTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void CreatePickup_HealthPickup_CreatesWithCorrectProperties(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Pickup> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("createPickup(1, 2, 3, 0, 100)");

        captures.Should().ContainSingle();
        captures[0].Position.Should().Be(new Vector3(1, 2, 3));
        captures[0].PickupType.Should().Be(PickupType.Health);
        captures[0].Health.Should().Be(100);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreatePickup_ArmorPickup_CreatesWithCorrectProperties(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Pickup> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("createPickup(0, 0, 0, 1, 50)");

        captures.Should().ContainSingle();
        captures[0].PickupType.Should().Be(PickupType.Armor);
        captures[0].Armor.Should().Be(50);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreatePickup_WeaponPickup_CreatesWithCorrectProperties(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Pickup> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("createPickup(0, 0, 0, 2, 31, 30000, 200)");

        captures.Should().ContainSingle();
        captures[0].PickupType.Should().Be(PickupType.Weapon);
        captures[0].WeaponType.Should().Be(WeaponId.M4);
        captures[0].Ammo.Should().Be(200);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreatePickup_CustomPickup_CreatesWithCorrectModel(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Pickup> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("createPickup(0, 0, 0, 3, 1212)");

        captures.Should().ContainSingle();
        captures[0].PickupType.Should().Be(PickupType.Custom);
        captures[0].Model.Should().Be(1212);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreatePickup_SetsRespawnTime(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Pickup> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("createPickup(0, 0, 0, 0, 100, 15000)");

        captures.Should().ContainSingle();
        captures[0].RespawnTime.Should().Be(15000);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPickupType_ReturnsHealthType(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 100).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("""
            assertPrint(tostring(getPickupType(testPickup)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPickupType_ReturnsArmorType(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Armor, 100).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("""
            assertPrint(tostring(getPickupType(testPickup)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPickupType_ReturnsWeaponType(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, WeaponId.Ak47, 100).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("""
            assertPrint(tostring(getPickupType(testPickup)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPickupAmmo_ReturnsAmmoForWeaponPickup(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, WeaponId.Ak47, 250).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("""
            assertPrint(tostring(getPickupAmmo(testPickup)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("250");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPickupAmount_ReturnsHealthAmount(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 75).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("""
            assertPrint(tostring(getPickupAmount(testPickup)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("75");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPickupAmount_ReturnsArmorAmount(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Armor, 50).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("""
            assertPrint(tostring(getPickupAmount(testPickup)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("50");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPickupWeapon_ReturnsWeaponId(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, WeaponId.M4, 100).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("""
            assertPrint(tostring(getPickupWeapon(testPickup)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(((int)WeaponId.M4).ToString());
        pickup.WeaponType.Should().Be(WeaponId.M4);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPickupType_ChangesToHealthPickup(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Armor, 100).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("setPickupType(testPickup, 0, 150)");

        pickup.PickupType.Should().Be(PickupType.Health);
        pickup.Health.Should().Be(150);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPickupType_ChangesToArmorPickup(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 100).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("setPickupType(testPickup, 1, 75)");

        pickup.PickupType.Should().Be(PickupType.Armor);
        pickup.Armor.Should().Be(75);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPickupType_ChangesToWeaponPickup(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 100).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("setPickupType(testPickup, 2, 31, 500)");

        pickup.PickupType.Should().Be(PickupType.Weapon);
        pickup.WeaponType.Should().Be(WeaponId.M4);
        pickup.Ammo.Should().Be(500);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPickupRespawnInterval_ReturnsRespawnTime(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 100) { RespawnTime = 12000 }.AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("""
            assertPrint(tostring(getPickupRespawnInterval(testPickup)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("12000");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPickupRespawnInterval_UpdatesRespawnTime(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 100).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("setPickupRespawnInterval(testPickup, 5000)");

        pickup.RespawnTime.Should().Be(5000);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPickupSpawned_ReturnsTrueByDefault(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 100).AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("""
            assertPrint(tostring(isPickupSpawned(testPickup)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        pickup.IsVisible.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPickupSpawned_ReturnsFalseWhenNotVisible(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 100) { IsVisible = false }.AssociateWith(sut);
        sut.AddGlobal("testPickup", pickup);

        sut.RunLuaScript("""
            assertPrint(tostring(isPickupSpawned(testPickup)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void UsePickup_AppliesHealthToPlayer(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        LightTestPlayer player,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Element>()));

        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 50) { IsUsable = true }.AssociateWith(sut);
        player.Health = 100;
        sut.AddGlobal("testPickup", pickup);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("usePickup(testPickup, testPlayer)");

        player.Health.Should().Be(150);
    }
}
