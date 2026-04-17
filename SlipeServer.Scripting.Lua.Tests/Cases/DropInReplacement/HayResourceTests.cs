using FluentAssertions;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Scripting.Lua.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases.DropInReplacement;

public class HayResourceTests
{
    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartHayResource_DoesNotThrow(
        IDropInReplacementResourceService service)
    {
        var exception = Record.Exception(() => service.StartResource("hay"));

        if (exception != null)
            throw new Exception(
                $"Starting 'hay' resource failed with {exception.GetType().Name}: {exception.Message}",
                exception);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartHayResource_HasNoScriptErrors(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("hay");

        server.ScriptErrors.Should().BeEmpty();
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartHayResource_LoadsLevelsSettingFromMeta(
        IDropInReplacementResourceService service,
        ISettingsRegistry registry)
    {
        service.StartResource("hay");

        var value = registry.Get("hay.levels");

        value.Should().NotBeNull();
        value!.IsNil.Should().BeFalse();
        value.DoubleValue.Should().Be(50);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartHayResource_LoadsBlocksSettingFromMeta(
        IDropInReplacementResourceService service,
        ISettingsRegistry registry)
    {
        service.StartResource("hay");

        var value = registry.Get("hay.blocks");

        value.Should().NotBeNull();
        value!.IsNil.Should().BeFalse();
        value.DoubleValue.Should().Be(245);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartHayResource_WithPlayerJoining_DoesNotThrow(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("hay");

        var exception = Record.Exception(() => server.JoinFakePlayer());

        if (exception != null)
            throw new Exception(
                $"Player joining after 'hay' started failed with {exception.GetType().Name}: {exception.Message}",
                exception);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartHayResource_WithPlayerJoining_HasNoScriptErrors(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("hay");

        server.JoinFakePlayer();

        server.ScriptErrors.Should().BeEmpty();
    }
}
