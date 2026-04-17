using FluentAssertions;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Scripting.Lua.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases.DropInReplacement;

public class FreeroamResourceTests
{
    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartFreeroamResource_DoesNotThrow(
        IDropInReplacementResourceService service)
    {
        var exception = Record.Exception(() => service.StartResource("freeroam"));

        if (exception != null)
            throw new Exception(
                $"Starting 'freeroam' resource failed with {exception.GetType().Name}: {exception.Message}",
                exception);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartFreeroamResource_WithPlayerJoining_DoesNotThrow(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        var startException = Record.Exception(() => service.StartResource("freeroam"));

        if (startException != null)
            throw new Exception(
                $"Starting 'freeroam' resource failed with {startException.GetType().Name}: {startException.Message}",
                startException);

        var joinException = Record.Exception(() => server.JoinFakePlayer());

        if (joinException != null)
            throw new Exception(
                $"Player joining after 'freeroam' started failed with {joinException.GetType().Name}: {joinException.Message}",
                joinException);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartFreeroamResource_WithPlayerJoining_HasNoScriptErrors(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("freeroam");

        server.JoinFakePlayer();

        server.ScriptErrors.Should().BeEmpty();
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartFreeroamResource_WithPlayerJoining_SetsNametagColor(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("freeroam");

        var player = server.JoinFakePlayer();

        player.NametagColor.Should().NotBeNull();
        player.NametagColor!.Value.R.Should().BeInRange(50, 255);
        player.NametagColor!.Value.G.Should().BeInRange(50, 255);
        player.NametagColor!.Value.B.Should().BeInRange(50, 255);
    }
}
