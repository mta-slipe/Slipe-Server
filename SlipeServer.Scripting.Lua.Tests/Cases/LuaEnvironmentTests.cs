using FluentAssertions;
using MoonSharp.Interpreter;
using SlipeServer.Lua;
using SlipeServer.Scripting;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class LuaEnvironmentTests
{
    // ── LoadString ────────────────────────────────────────────────────────────

    [Theory]
    [ScriptingAutoDomainData]
    public void LoadString_ExecutesLuaCode(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var env = sut.CreateEnvironment("test");

        env.LoadString("assertPrint('hello')");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void LoadString_MultipleLoads_StateAccumulatesAcrossCalls(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var env = sut.CreateEnvironment("test");

        env.LoadString("x = 10");
        env.LoadString("assertPrint(tostring(x))");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("10");
    }

    // ── SetGlobal / RemoveGlobal ──────────────────────────────────────────────

    [Theory]
    [ScriptingAutoDomainData]
    public void SetGlobal_MakesValueAvailableInLua(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var env = sut.CreateEnvironment("test");
        env.SetGlobal("greeting", "hi there");

        env.LoadString("assertPrint(greeting)");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hi there");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RemoveGlobal_MakesValueUnavailableInLua(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var env = sut.CreateEnvironment("test");
        env.SetGlobal("greeting", "hi there");
        env.RemoveGlobal("greeting");

        env.LoadString("assertPrint(type(greeting))");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }

    // ── CallFunction ──────────────────────────────────────────────────────────

    [Theory]
    [ScriptingAutoDomainData]
    public void CallFunction_InvokesNamedFunction(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var env = sut.CreateEnvironment("test");
        env.LoadString("function greet() assertPrint('called') end");

        env.CallFunction("greet");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("called");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CallFunction_ReturnsNumberValue(IMtaServer sut)
    {
        var env = sut.CreateEnvironment("test");
        env.LoadString("function add(a, b) return a + b end");

        var result = env.CallFunction("add", 3, 4);

        result.Should().ContainSingle().Which.Number.Should().Be(7);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CallFunction_ReturnsStringValue(IMtaServer sut)
    {
        var env = sut.CreateEnvironment("test");
        env.LoadString("function echo(s) return s end");

        var result = env.CallFunction("echo", "slipe");

        result.Should().ContainSingle().Which.String.Should().Be("slipe");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CallFunction_ReturnsMultipleValues(IMtaServer sut)
    {
        var env = sut.CreateEnvironment("test");
        env.LoadString("function multi() return 1, 2, 3 end");

        var result = env.CallFunction("multi");

        result.Should().HaveCount(3);
        result[0].Number.Should().Be(1);
        result[1].Number.Should().Be(2);
        result[2].Number.Should().Be(3);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CallFunction_PassesArgumentsToFunction(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var env = sut.CreateEnvironment("test");
        env.LoadString("function show(a, b) assertPrint(tostring(a) .. ':' .. b) end");

        env.CallFunction("show", 42, "world");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("42:world");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CallFunction_ThrowsArgumentException_WhenFunctionNotFound(IMtaServer sut)
    {
        var env = sut.CreateEnvironment("test");

        var act = () => env.CallFunction("nonExistentFunction");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*nonExistentFunction*");
    }

    // ── resourceRoot ──────────────────────────────────────────────────────────

    [Theory]
    [ScriptingAutoDomainData]
    public void ResourceRoot_IsSet_WhenEnvironmentHasResource(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "test-resource");
        var env = sut.CreateEnvironment("env-with-resource", resource);

        env.LoadString("assertPrint(type(resourceRoot))");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("userdata");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResourceRoot_IsNil_WhenEnvironmentHasNoResource(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var env = sut.CreateEnvironment("env-without-resource");

        env.LoadString("assertPrint(type(resourceRoot))");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }

    // ── Unload ────────────────────────────────────────────────────────────────

    [Theory]
    [ScriptingAutoDomainData]
    public void Unload_RemovesCommandHandlers(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);

        var env = sut.CreateEnvironment("test");
        env.LoadString("""
            addCommandHandler("ping", function(p, cmd)
                assertPrint("ping")
            end)
            """);

        player.TriggerCommand("ping", []);
        assertDataProvider.AssertPrints.Should().ContainSingle();

        env.Unload();
        player.TriggerCommand("ping", []);

        assertDataProvider.AssertPrints.Should().ContainSingle("handler must not fire after Unload");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Unload_KillsTimers(IMtaServer sut)
    {
        var timerService = sut.GetRequiredService<ScriptTimerService>();
        var env = sut.CreateEnvironment("test");
        env.LoadString("setTimer(function() end, 5000, 1)");

        timerService.GetTimers().Should().HaveCount(1);

        env.Unload();

        timerService.GetTimers().Should().BeEmpty();
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void Unload_RemovesEventHandlers(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var eventRuntime = sut.GetRequiredService<IScriptEventRuntime>();
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "test-resource");

        var env = sut.CreateEnvironment("test", resource);
        env.LoadString("""
            addEvent("onTestEvent", false)
            addEventHandler("onTestEvent", root, function()
                assertPrint("fired")
            end)
            """);

        eventRuntime.TriggerCustomEvent("onTestEvent", sut.RootElement);
        assertDataProvider.AssertPrints.Should().ContainSingle();

        env.Unload();
        eventRuntime.TriggerCustomEvent("onTestEvent", sut.RootElement);

        assertDataProvider.AssertPrints.Should().ContainSingle("handler must not fire after Unload");
    }

    // ── Isolation ─────────────────────────────────────────────────────────────

    [Theory]
    [ScriptingAutoDomainData]
    public void Environments_DoNotShareGlobals(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var env1 = sut.CreateEnvironment("env1");
        var env2 = sut.CreateEnvironment("env2");

        env1.SetGlobal("shared", "from-env1");
        env2.SetGlobal("shared", "from-env2");

        env1.LoadString("assertPrint(shared)");
        env2.LoadString("assertPrint(shared)");

        assertDataProvider.AssertPrints[0].Should().Be("from-env1");
        assertDataProvider.AssertPrints[1].Should().Be("from-env2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Environments_DoNotShareFunctions(IMtaServer sut)
    {
        var env1 = sut.CreateEnvironment("env1");
        env1.LoadString("function secret() return 42 end");

        var env2 = sut.CreateEnvironment("env2");
        var act = () => env2.CallFunction("secret");

        act.Should().Throw<ArgumentException>();
    }

    // ── LuaEnvironmentService lookups ─────────────────────────────────────────

    [Theory]
    [ScriptingAutoDomainData]
    public void GetEnvironment_ByIdentifier_ReturnsRegisteredEnvironment(IMtaServer sut)
    {
        var env = sut.CreateEnvironment("my-env");

        var found = sut.GetEnvironmentService().GetEnvironment("my-env");

        found.Should().BeSameAs(env);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetEnvironment_ByIdentifier_ReturnsNull_WhenNotFound(IMtaServer sut)
    {
        var found = sut.GetEnvironmentService().GetEnvironment("does-not-exist");

        found.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetEnvironment_ByResource_ReturnsRegisteredEnvironment(IMtaServer sut)
    {
        var root = sut.GetRequiredService<IRootElement>();
        var resource = new Resource(sut, root, "res");
        var env = sut.CreateEnvironment("env-res", resource);

        var found = sut.GetEnvironmentService().GetEnvironment(resource);

        found.Should().BeSameAs(env);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetEnvironment_ByContext_ReturnsRegisteredEnvironment(IMtaServer sut)
    {
        var env = sut.CreateEnvironment("ctx-env");

        var found = sut.GetEnvironmentService().GetEnvironment(env.ExecutionContext);

        found.Should().BeSameAs(env);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAllEnvironments_ReturnsAllRegisteredEnvironments(IMtaServer sut)
    {
        var env1 = sut.CreateEnvironment("all-1");
        var env2 = sut.CreateEnvironment("all-2");
        var env3 = sut.CreateEnvironment("all-3");

        sut.GetEnvironmentService().GetAllEnvironments()
            .Should().Contain([env1, env2, env3]);
    }

    // ── Integration: LoadScript registers environment ─────────────────────────

    [Theory]
    [ScriptingAutoDomainData]
    public void LoadScript_RegistersEnvironment_InEnvironmentService(IMtaServer sut)
    {
        var luaService = sut.GetRequiredService<LuaService>();
        luaService.LoadScript("integration-test", "-- no-op");

        sut.GetEnvironmentService().GetEnvironment("integration-test").Should().NotBeNull();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void UnloadScript_UnregistersEnvironment_FromEnvironmentService(IMtaServer sut)
    {
        var luaService = sut.GetRequiredService<LuaService>();
        luaService.LoadScript("to-unload", "-- no-op");
        luaService.UnloadScript("to-unload");

        sut.GetEnvironmentService().GetEnvironment("to-unload").Should().BeNull();
    }
}
