using FluentAssertions;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Scripting.Lua.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases.DropInReplacement;

public class HayResourceTests
{
    private static string ResourceDirectory => Path.Combine(AppContext.BaseDirectory, "Resources");

    [Fact]
    public void StartHayResource_DoesNotThrow()
    {
        var server = new DropInReplacementTestingServer(ResourceDirectory);
        var service = server.GetRequiredService<IDropInReplacementResourceService>();

        var exception = Record.Exception(() => service.StartResource("hay"));

        if (exception != null)
            throw new Exception(
                $"Starting 'hay' resource failed with {exception.GetType().Name}: {exception.Message}",
                exception);
    }

    [Fact]
    public void StartHayResource_HasNoScriptErrors()
    {
        var server = new DropInReplacementTestingServer(ResourceDirectory);
        var service = server.GetRequiredService<IDropInReplacementResourceService>();
        service.StartResource("hay");

        server.ScriptErrors.Should().BeEmpty();
    }

    [Fact]
    public void StartHayResource_LoadsLevelsSettingFromMeta()
    {
        var server = new DropInReplacementTestingServer(ResourceDirectory);
        var registry = server.GetRequiredService<ISettingsRegistry>();
        var service = server.GetRequiredService<IDropInReplacementResourceService>();
        service.StartResource("hay");

        var value = registry.Get("hay.levels");

        value.Should().NotBeNull();
        value!.IsNil.Should().BeFalse();
        value.DoubleValue.Should().Be(50);
    }

    [Fact]
    public void StartHayResource_LoadsBlocksSettingFromMeta()
    {
        var server = new DropInReplacementTestingServer(ResourceDirectory);
        var registry = server.GetRequiredService<ISettingsRegistry>();
        var service = server.GetRequiredService<IDropInReplacementResourceService>();
        service.StartResource("hay");

        var value = registry.Get("hay.blocks");

        value.Should().NotBeNull();
        value!.IsNil.Should().BeFalse();
        value.DoubleValue.Should().Be(245);
    }

    [Fact]
    public void StartHayResource_WithPlayerJoining_DoesNotThrow()
    {
        var server = new DropInReplacementTestingServer(ResourceDirectory);
        var service = server.GetRequiredService<IDropInReplacementResourceService>();
        service.StartResource("hay");

        var exception = Record.Exception(() => server.JoinFakePlayer());

        if (exception != null)
            throw new Exception(
                $"Player joining after 'hay' started failed with {exception.GetType().Name}: {exception.Message}",
                exception);
    }

    [Fact]
    public void StartHayResource_WithPlayerJoining_HasNoScriptErrors()
    {
        var server = new DropInReplacementTestingServer(ResourceDirectory);
        var service = server.GetRequiredService<IDropInReplacementResourceService>();
        service.StartResource("hay");

        server.JoinFakePlayer();

        server.ScriptErrors.Should().BeEmpty();
    }
}
