using FluentAssertions;
using SlipeServer.Lua;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Resources;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class ResourceFunctionTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void GetResourceFromName_ReturnsProviderResource_WhenNotStarted(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var provider = sut.GetRequiredService<TestResourceProvider>();
        var luaService = sut.GetRequiredService<LuaService>();
        var resource = new Resource(sut, sut.RootElement, "providerResource");
        provider.AddResource(resource);
        luaService.AddGlobal("expectedResource", resource);

        sut.RunLuaScript("""
            local actualResource = getResourceFromName("providerResource")
            assert(actualResource ~= nil)
            assertPrint(tostring(actualResource == expectedResource))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void StartResource_StartsProviderResource(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var provider = sut.GetRequiredService<TestResourceProvider>();
        var resourceService = sut.GetRequiredService<IResourceService>();
        var luaService = sut.GetRequiredService<LuaService>();
        var resource = new Resource(sut, sut.RootElement, "startableResource");
        provider.AddResource(resource);
        luaService.AddGlobal("startableResource", resource);

        sut.RunLuaScript("""
            local result = startResource(startableResource)
            assert(result == true)
            assertPrint(tostring(result))
            assertPrint(getResourceState(startableResource))
            """);

        assertDataProvider.AssertPrints.Should().Equal("true", "running");
        resourceService.StartedResources.Should().ContainSingle(r => r.Name == "startableResource");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void StopResource_StopsRunningResource(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var provider = sut.GetRequiredService<TestResourceProvider>();
        var resourceService = sut.GetRequiredService<IResourceService>();
        var luaService = sut.GetRequiredService<LuaService>();
        var resource = new Resource(sut, sut.RootElement, "stoppableResource");
        provider.AddResource(resource);
        resourceService.StartResource(resource.Name);
        luaService.AddGlobal("stoppableResource", resource);

        sut.RunLuaScript("""
            local result = stopResource(stoppableResource)
            assert(result == true)
            assertPrint(tostring(result))
            assertPrint(getResourceState(stoppableResource))
            """);

        assertDataProvider.AssertPrints.Should().Equal("true", "loaded");
        resourceService.StartedResources.Should().BeEmpty();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RestartResource_RestartsRunningResource(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var provider = sut.GetRequiredService<TestResourceProvider>();
        var resourceService = sut.GetRequiredService<IResourceService>();
        var luaService = sut.GetRequiredService<LuaService>();
        var resource = new Resource(sut, sut.RootElement, "restartableResource");
        provider.AddResource(resource);
        resourceService.StartResource(resource.Name);
        luaService.AddGlobal("restartableResource", resource);

        var startedCount = 0;
        var stoppedCount = 0;
        resourceService.ResourceStarted += _ => startedCount++;
        resourceService.ResourceStopped += _ => stoppedCount++;

        sut.RunLuaScript("""
            local result = restartResource(restartableResource)
            assert(result == true)
            assertPrint(tostring(result))
            assertPrint(getResourceState(restartableResource))
            """);

        assertDataProvider.AssertPrints.Should().Equal("true", "running");
        resourceService.StartedResources.Should().ContainSingle(r => r.Name == "restartableResource");
        startedCount.Should().Be(1);
        stoppedCount.Should().Be(1);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetResources_ReturnsProviderResources(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var provider = sut.GetRequiredService<TestResourceProvider>();
        provider.AddResource(new Resource(sut, sut.RootElement, "resourceOne"));
        provider.AddResource(new Resource(sut, sut.RootElement, "resourceTwo"));

        sut.RunLuaScript("""
            local resources = getResources()
            assert(#resources == 2)
            local names = { getResourceName(resources[1]), getResourceName(resources[2]) }
            table.sort(names)
            assertPrint(table.concat(names, ","))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("resourceOne,resourceTwo");
    }
}
