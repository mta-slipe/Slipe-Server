using FluentAssertions;
using Moq;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Mappers;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Mappers;

public class FromLuaValueMapperTests
{
    [Fact]
    public void LuaTableMapsToDictionary()
    {
        var luaValue = new LuaValue(new Dictionary<LuaValue, LuaValue>()
        {
            ["testkey"] = "testvalue",
            ["testkey2"] = "testvalue2",
        });

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(typeof(Dictionary<string, string>), luaValue);

        result.Should().BeEquivalentTo(new Dictionary<string, string>()
        {
            ["testkey"] = "testvalue",
            ["testkey2"] = "testvalue2",
        });
    }

    [Fact]
    public void LuaTableMapsToIEnumerable()
    {
        var luaValue = new LuaValue(new Dictionary<LuaValue, LuaValue>()
        {
            [1] = "testvalue",
            [2] = "testvalue2",
        });

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(typeof(IEnumerable<string>), luaValue);

        result.Should().BeEquivalentTo(new string[]
        {
            "testvalue",
            "testvalue2",
        });
    }

    [Fact]
    public void LuaValueMapsToString()
    {
        var luaValue = new LuaValue("value");

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(typeof(string), luaValue);

        result.Should().BeEquivalentTo("value");
    }

    [Fact]
    public void LuaValueMapsToBool()
    {
        var luaValue = new LuaValue(true);

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(typeof(bool), luaValue);

        result.Should().BeEquivalentTo(true);
    }

    [Theory]
    [InlineData(typeof(int), 1f, 1)]
    [InlineData(typeof(float), 1f, 1f)]
    [InlineData(typeof(double), 1f, 1d)]
    public void FloatCrossMappingWorksProperly(Type type, float source, object target)
    {
        var luaValue = new LuaValue(source);

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(type, luaValue);

        result.Should().BeEquivalentTo(target);
    }

    [Theory]
    [InlineData(typeof(int), 1d, 1)]
    [InlineData(typeof(float), 1d, 1f)]
    [InlineData(typeof(double), 1d, 1d)]
    public void DoubleCrossMappingWorksProperly(Type type, int source, object target)
    {
        var luaValue = new LuaValue(source);

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(type, luaValue);

        result.Should().BeEquivalentTo(target);
    }

    [Theory]
    [InlineData(typeof(int), 1, 1)]
    [InlineData(typeof(float), 1, 1f)]
    [InlineData(typeof(double), 1, 1d)]
    public void IntCrossMappingWorksProperly(Type type, int source, object target)
    {
        var luaValue = new LuaValue(source);

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(type, luaValue);

        result.Should().BeEquivalentTo(target);
    }

    [Fact]
    public void VectorMapperWorks()
    {
        var luaValue = new LuaValue(new Dictionary<LuaValue, LuaValue>()
        {
            ["X"] = 0.5f,
            ["Y"] = 1,
            ["Z"] = 1.5d
        });

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(typeof(Vector3), luaValue);

        result.Should().BeEquivalentTo(new Vector3(0.5f, 1f, 1.5f));
    }

    [Fact]
    public void ReflectionMapperWorks()
    {
        var luaValue = new LuaValue(new Dictionary<LuaValue, LuaValue>()
        {
            ["X"] = 0.5f,
            ["Y"] = 1,
            ["Z"] = 1.5d
        });

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(typeof(TestVector3), luaValue);

        result.Should().BeEquivalentTo(new TestVector3(0.5f, 1f, 1.5f));
    }

    [Fact]
    public void LuaValueMapsToEnum()
    {
        var luaValue = new LuaValue(30);

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(typeof(WeaponId), luaValue);

        result.Should().BeEquivalentTo(WeaponId.Ak47);
    }

    [Fact]
    public void LuaValueMapsToElement()
    {
        var id = 1u;
        var luaValue = LuaValue.CreateElement(id);
        var element = new Element();

        var elementCollectionMock = new Mock<IElementCollection>();
        elementCollectionMock
            .Setup(x => x.Get(id))
            .Returns(element);

        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(typeof(Element), luaValue);

        result.Should().Be(element);
    }

    [Fact]
    public void LuaValueMapsToElementSubClass()
    {
        var id = 1u;
        var luaValue = LuaValue.CreateElement(id);
        var element = new Vehicle(0u, Vector3.Zero);

        var elementCollectionMock = new Mock<IElementCollection>();
        elementCollectionMock
            .Setup(x => x.Get(id))
            .Returns(element);

        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        var result = mapper.Map(typeof(Vehicle), luaValue);

        result.Should().Be(element);
    }

    [Fact]
    public void LuaValueMapsUsingDefinedMapper()
    {
        var luaValue = new LuaValue(5);

        var elementCollectionMock = new Mock<IElementCollection>();
        var mapper = new FromLuaValueMapper(elementCollectionMock.Object);
        mapper.DefineMapper(typeof(TestVector3), x => new TestVector3(x.IntegerValue ?? 0, x.IntegerValue ?? 0, x.IntegerValue ?? 0));

        var result = mapper.Map(typeof(TestVector3), luaValue);

        result.Should().BeEquivalentTo(new TestVector3(5, 5, 5));
    }
}
