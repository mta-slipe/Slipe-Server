using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Scripting.Lua.Tests.Tools;
using System;
using System.IO;
using Xunit;

namespace SlipeServer.Scripting.Lua.Tests.Cases.DropInReplacement;

public class FreeroamResourceTests
{
    private static string ResourceDirectory => Path.Combine(AppContext.BaseDirectory, "Resources");

    [Fact]
    public void StartFreeroamResource_DoesNotThrow()
    {
        var server = new DropInReplacementTestingServer(ResourceDirectory);
        var service = server.GetRequiredService<IDropInReplacementResourceService>();
        service.StartResource("freeroam");

        var exception = Record.Exception(() => service.StartResource("freeroam"));

        if (exception != null)
            throw new Exception(
                $"Starting 'freeroam' resource failed with {exception.GetType().Name}: {exception.Message}",
                exception);
    }

    [Fact]
    public void StartFreeroamResource_WithPlayerJoining_DoesNotThrow()
    {
        var server = new DropInReplacementTestingServer(ResourceDirectory);
        var service = server.GetRequiredService<IDropInReplacementResourceService>();

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
}
