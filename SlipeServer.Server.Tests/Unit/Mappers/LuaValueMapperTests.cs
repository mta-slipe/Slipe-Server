using FluentAssertions;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Mappers;

public class LuaValueMapperTests
{
    [Theory]
    [InlineData((byte)5, 5)]
    [InlineData((sbyte)5, 5)]
    [InlineData((ushort)5, 5)]
    [InlineData((short)5, 5)]
    [InlineData((int)5, 5)]
    public void IntMapsToLuaValue(object value, int expected)
    {
        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.Should().BeEquivalentTo(new LuaValue(expected));
    }

    [Fact]
    public void FloatMapsToLuaValue()
    {
        var value = 5f;

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.Should().BeEquivalentTo(new LuaValue(5f));
    }

    [Fact]
    public void DoubleMapsToLuaValue()
    {
        var value = 5d;

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.Should().BeEquivalentTo(new LuaValue(5d));
    }

    [Fact]
    public void StringMapsToLuaValue()
    {
        var value = "value";

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.Should().BeEquivalentTo(new LuaValue("value"));
    }

    [Fact]
    public void BooleanMapsToLuaValue()
    {
        var value = true;

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.Should().BeEquivalentTo(new LuaValue(true));
    }

    [Fact]
    public void ElementMapsToLuaValue()
    {
        var value = new Element() { Id = (ElementId)36 };

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.Should().BeEquivalentTo(new LuaValue((ElementId)36u));
    }

    [Fact]
    public void UintMapsToLuaValue()
    {
        var value = 36u;

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.Should().BeEquivalentTo(new LuaValue((ElementId)36u));
    }

    [Fact]
    public void IEnumerableMapsToLuaValue()
    {
        var value = new List<string>() { "A", "B", "C" };

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.TableValue.Should().BeEquivalentTo(new LuaValue(new LuaValue[] {
            new LuaValue("A"),
            new LuaValue("B"),
            new LuaValue("C")
        }).TableValue);
    }

    [Fact]
    public void DictionaryMapsToLuaValue()
    {
        var value = new Dictionary<string, string>()
        {
            ["A"] = "a",
            ["B"] = "b",
            ["C"] = "c",
        };

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.TableValue.Should().BeEquivalentTo(new Dictionary<LuaValue, LuaValue>()
        {
            ["A"] = "a",
            ["B"] = "b",
            ["C"] = "c",
        });
    }

    [Fact]
    public void NullMapsToLuaValue()
    {
        object? value = null;

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.Should().BeEquivalentTo(new LuaValue());
    }

    [Fact]
    public void NullStringToLuaValue()
    {
        string? value = null;

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.Should().BeEquivalentTo(new LuaValue());
    }

    [Fact]
    public void ReflectionMappingTest()
    {
        var value = new TestVector3(1, 2, 3);

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.TableValue.Should().BeEquivalentTo(new Dictionary<LuaValue, LuaValue>()
        {
            ["X"] = 1f,
            ["Y"] = 2f,
            ["Z"] = 3f
        });
    }

    [Fact]
    public void DefinedStructMapperTest()
    {
        var value = new TestVector3(1, 2, 3);

        var mapper = new LuaValueMapper();
        mapper.DefineStructMapper<TestVector3>(x => new Dictionary<LuaValue, LuaValue>()
        {
            ["X"] = x.X + 1,
            ["Y"] = x.Y + 1,
            ["Z"] = x.Z + 1
        });

        var result = mapper.Map(value);

        result.TableValue.Should().BeEquivalentTo(new Dictionary<LuaValue, LuaValue>()
        {
            ["X"] = 2f,
            ["Y"] = 3f,
            ["Z"] = 4f
        });
    }

    [Fact]
    public void DefinedMapperTest()
    {
        var value = new StringBuilder();

        var mapper = new LuaValueMapper();
        mapper.DefineMapper<StringBuilder>(x => 5);

        var result = mapper.Map(value);

        result.Should().BeEquivalentTo(new LuaValue(5));
    }

    [Fact]
    public void EnumerableMapperTests()
    {
        var value = new string[] { "A", "B", "C" };

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.TableValue.Should().BeEquivalentTo(new Dictionary<LuaValue, LuaValue>()
        {
            [1] = "A",
            [2] = "B",
            [3] = "C"
        });
    }

    [Fact]
    public void DictionaryMapperTests()
    {
        var value = new Dictionary<string, string> { 
            ["a"] = "A", 
            ["b"] = "B", 
            ["c"] = "C" 
        };

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.TableValue.Should().BeEquivalentTo(new Dictionary<LuaValue, LuaValue>()
        {
            ["a"] = "A",
            ["b"] = "B",
            ["c"] = "C"
        });
    }

    [Fact]
    public void LuaValueMappableTest()
    {
        var value = new LuaMappableVector3(1, 2, 3);

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.TableValue.Should().BeEquivalentTo(new Dictionary<LuaValue, LuaValue>()
        {
            ["X"] = 2f,
            ["Y"] = 4f,
            ["Z"] = 6f,
        });
    }

    [Fact]
    public void LuaValueMapsToItself()
    {
        var value = new LuaValue(5);

        var mapper = new LuaValueMapper();
        var result = mapper.Map(value);

        result.Should().Be(value);
    }
}
