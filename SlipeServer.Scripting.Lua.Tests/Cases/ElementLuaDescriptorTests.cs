using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

/// <summary>
/// Tests for <see cref="SlipeServer.Lua.ElementLuaDescriptor"/> — verifies that element
/// UserData objects are accessible from Lua using MTA-style OOP camelCase property names,
/// that vehicleType returns the correct MTA string, and that a Lua Vector3 table can be
/// assigned to a Vector3 property.
/// </summary>
public class ElementLuaDescriptorTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void Interior_Read_CamelCase_ReturnsCorrectValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero);
        vehicle.Interior = 3;
        sut.AddGlobal("testElement", vehicle);

        sut.RunLuaScript("assertPrint(tostring(testElement.interior))");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("3");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Dimension_Read_CamelCase_ReturnsCorrectValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero);
        vehicle.Dimension = 7;
        sut.AddGlobal("testElement", vehicle);

        sut.RunLuaScript("assertPrint(tostring(testElement.dimension))");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("7");
    }


    [Theory]
    [ScriptingAutoDomainData]
    public void Interior_Read_PascalCase_StillReturnsCorrectValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero);
        vehicle.Interior = 5;
        sut.AddGlobal("testElement", vehicle);

        sut.RunLuaScript("assertPrint(tostring(testElement.Interior))");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("5");
    }


    [Theory]
    [ScriptingAutoDomainData]
    public void Interior_Write_CamelCase_SetsProperty(IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero);
        vehicle.Interior = 0;
        sut.AddGlobal("testElement", vehicle);

        sut.RunLuaScript("testElement.interior = 4");

        vehicle.Interior.Should().Be(4);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Dimension_Write_CamelCase_SetsProperty(IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero);
        vehicle.Dimension = 0;
        sut.AddGlobal("testElement", vehicle);

        sut.RunLuaScript("testElement.dimension = 9");

        vehicle.Dimension.Should().Be(9);
    }


    [Theory]
    [ScriptingAutoDomainData]
    public void VehicleType_Automobile_ReturnsAutomobileString(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero); // Infernus = Automobile
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("assertPrint(testVehicle.vehicleType)");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("Automobile");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void VehicleType_Motorcycle_ReturnsBikeString(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(461, Vector3.Zero); // PCJ600 = Motorcycle
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("assertPrint(testVehicle.vehicleType)");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("Bike");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void VehicleType_Bmx_ReturnsBmxString(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(481, Vector3.Zero); // BMX
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("assertPrint(testVehicle.vehicleType)");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("BMX");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void VehicleType_QuadBike_ReturnsQuadBikeString(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(471, Vector3.Zero); // Quadbike
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("assertPrint(testVehicle.vehicleType)");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("Quad Bike");
    }


    [Theory]
    [ScriptingAutoDomainData]
    public void Velocity_Write_LuaVector3Table_SetsVelocityProperty(IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("testVehicle.velocity = Vector3(1, 2, 3)");

        vehicle.Velocity.X.Should().BeApproximately(1f, 0.001f);
        vehicle.Velocity.Y.Should().BeApproximately(2f, 0.001f);
        vehicle.Velocity.Z.Should().BeApproximately(3f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Velocity_Write_NegativeValues_SetsVelocityCorrectly(IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero);
        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("testVehicle.velocity = Vector3(0, 0, -0.01)");

        vehicle.Velocity.X.Should().BeApproximately(0f, 0.001f);
        vehicle.Velocity.Y.Should().BeApproximately(0f, 0.001f);
        vehicle.Velocity.Z.Should().BeApproximately(-0.01f, 0.001f);
    }
}
