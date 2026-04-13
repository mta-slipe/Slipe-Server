using FluentAssertions;
using MoonSharp.Interpreter;
using SlipeServer.Lua;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class LuaCallTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void Call_InvokesExportedFunction(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "callee");
        resource.Exports.Add("greet");

        var calleeEnv = sut.CreateEnvironment("callee", resource);
        calleeEnv.LoadString("function greet() assertPrint('hello from callee') end");

        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("""
            local res = getResourceFromName("callee")
            call(res, "greet")
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello from callee");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Call_PassesArgumentsToExportedFunction(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "callee");
        resource.Exports.Add("echo");

        var calleeEnv = sut.CreateEnvironment("callee", resource);
        calleeEnv.LoadString("function echo(a, b) assertPrint(tostring(a) .. ':' .. b) end");

        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("""
            local res = getResourceFromName("callee")
            call(res, "echo", 42, "world")
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("42:world");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Call_ReturnsValueFromExportedFunction(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "callee");
        resource.Exports.Add("add");

        var calleeEnv = sut.CreateEnvironment("callee", resource);
        calleeEnv.LoadString("function add(a, b) return a + b end");

        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("""
            local res = getResourceFromName("callee")
            assertPrint(tostring(call(res, "add", 3, 4)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("7");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Call_ReturnsMultipleValuesFromExportedFunction(IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "callee");
        resource.Exports.Add("multi");

        var calleeEnv = sut.CreateEnvironment("callee", resource);
        calleeEnv.LoadString("function multi() return 1, 2, 3 end");

        // Call via C# using CallWithSource indirectly through another environment
        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("""
            function getMulti()
                return call(getResourceFromName("callee"), "multi")
            end
            """);
        var results = callerEnv.CallFunction("getMulti");

        results.Should().HaveCount(3);
        results[0].Number.Should().Be(1);
        results[1].Number.Should().Be(2);
        results[2].Number.Should().Be(3);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Call_ReturnsFalse_WhenFunctionNotExported(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "callee");
        // Note: "secret" is NOT added to Exports

        var calleeEnv = sut.CreateEnvironment("callee", resource);
        calleeEnv.LoadString("function secret() return 'hidden' end");

        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("""
            local res = getResourceFromName("callee")
            local result = call(res, "secret")
            assertPrint(tostring(result))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Call_ReturnsFalse_WhenResourceIsNil(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("""
            local result = call(nil, "someFunc")
            assertPrint(tostring(result))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Call_SetsSourceResourceInCallee(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();

        var calleeResource = new Resource(sut, root, "callee");
        calleeResource.Exports.Add("whoCalledMe");
        var calleeEnv = sut.CreateEnvironment("callee", calleeResource);
        calleeEnv.LoadString("""
            function whoCalledMe()
                if sourceResource ~= nil then
                    assertPrint("has-source")
                else
                    assertPrint("no-source")
                end
            end
            """);

        var callerResource = new Resource(sut, root, "caller");
        var callerEnv = sut.CreateEnvironment("caller", callerResource);
        callerEnv.LoadString("""
            call(getResourceFromName("callee"), "whoCalledMe")
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("has-source");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Call_RestoresSourceResourceAfterCall(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();

        var calleeResource = new Resource(sut, root, "callee");
        calleeResource.Exports.Add("noop");
        var calleeEnv = sut.CreateEnvironment("callee", calleeResource);
        calleeEnv.LoadString("function noop() end");

        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("""
            call(getResourceFromName("callee"), "noop")
            assertPrint(tostring(sourceResource))
            """);

        // sourceResource in the caller env should remain nil (it was never set there)
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }


    [Theory]
    [ScriptingAutoDomainData]
    public void GetResourceFromName_ReturnsNonNilResource_WhenExists(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "myResource");
        sut.CreateEnvironment("myResource", resource);

        var env = sut.CreateEnvironment("caller");
        env.LoadString("""
            local res = getResourceFromName("myResource")
            assertPrint(tostring(res ~= nil))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetResourceFromName_ReturnsNil_WhenNotFound(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var env = sut.CreateEnvironment("caller");
        env.LoadString("""
            local res = getResourceFromName("nonExistent")
            assertPrint(tostring(res))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }


    [Theory]
    [ScriptingAutoDomainData]
    public void GetThisResource_ReturnsCurrentResource(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "myResource");

        var env = sut.CreateEnvironment("myResource", resource);
        env.LoadString("""
            local res = getThisResource()
            assertPrint(tostring(res ~= nil))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetThisResource_ReturnsNil_WhenNoResourceOwner(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var env = sut.CreateEnvironment("noResource");
        env.LoadString("""
            local res = getThisResource()
            assertPrint(tostring(res))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetThisResource_MatchesGetResourceFromName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "selfResource");
        resource.Exports.Add("getSelf");

        var env = sut.CreateEnvironment("selfResource", resource);
        env.LoadString("""
            function getSelf()
                local self = getThisResource()
                local byName = getResourceFromName("selfResource")
                assertPrint(tostring(self == byName))
            end
            """);

        env.CallFunction("getSelf");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }
    // ── exports table ─────────────────────────────────────────────────────────

    [Theory]
    [ScriptingAutoDomainData]
    public void Exports_ColonSyntax_InvokesExportedFunction(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "callee");
        resource.Exports.Add("greet");

        var calleeEnv = sut.CreateEnvironment("callee", resource);
        calleeEnv.LoadString("function greet() assertPrint('hello via exports') end");

        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("exports.callee:greet()");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello via exports");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Exports_ColonSyntax_PassesArgumentsAndReturnsValues(IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "callee");
        resource.Exports.Add("add");

        var calleeEnv = sut.CreateEnvironment("callee", resource);
        calleeEnv.LoadString("function add(a, b) return a + b end");

        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("""
            function getResult()
                return exports.callee:add(3, 4)
            end
            """);

        var result = callerEnv.CallFunction("getResult");

        result.Should().ContainSingle().Which.Number.Should().Be(7);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Exports_ColonSyntax_WithMixedArguments(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "callee");
        resource.Exports.Add("exportedFunction");

        var calleeEnv = sut.CreateEnvironment("callee", resource);
        calleeEnv.LoadString("""
            function exportedFunction(a, b, c)
                assertPrint(tostring(a) .. "," .. tostring(b) .. "," .. tostring(c))
            end
            """);

        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("""exports.callee:exportedFunction(1, "2", "three")""");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1,2,three");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Exports_ColonSyntax_ReturnsFalse_WhenFunctionNotExported(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "callee");

        var calleeEnv = sut.CreateEnvironment("callee", resource);
        calleeEnv.LoadString("function secret() return 'hidden' end");

        var callerEnv = sut.CreateEnvironment("caller");
        callerEnv.LoadString("""
            local result = exports.callee:secret()
            assertPrint(tostring(result))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }
}
