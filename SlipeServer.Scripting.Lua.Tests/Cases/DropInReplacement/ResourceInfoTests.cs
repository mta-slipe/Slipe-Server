using FluentAssertions;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Scripting.Lua.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases.DropInReplacement;

public class ResourceInfoTests
{
    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartGetResourceInfoTestResource_DoesNotThrow(
        IDropInReplacementResourceService service)
    {
        var exception = Record.Exception(() => service.StartResource("getresourceinfo_test"));

        if (exception != null)
            throw new Exception(
                $"Starting 'getresourceinfo_test' resource failed with {exception.GetType().Name}: {exception.Message}",
                exception);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartGetResourceInfoTestResource_HasNoScriptErrors(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("getresourceinfo_test");

        server.ScriptErrors.Should().BeEmpty();
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartGetResourceInfoTestResource_InfoIsParsedFromMetaXml(
        IDropInReplacementResourceService service)
    {
        var resource = service.StartResource("getresourceinfo_test");

        resource.Should().NotBeNull();
        resource!.Info.Should().ContainKey("version").WhoseValue.Should().Be("2.5");
        resource.Info.Should().ContainKey("author").WhoseValue.Should().Be("TestAuthor");
        resource.Info.Should().ContainKey("type").WhoseValue.Should().Be("script");
    }
}
