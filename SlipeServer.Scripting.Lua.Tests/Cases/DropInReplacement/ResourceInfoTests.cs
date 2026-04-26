using FluentAssertions;
using SlipeServer.DropInReplacement.MixedResources;
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

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartMetaParseTestResource_ParsesExtendedMetaTags(
        IDropInReplacementResourceService service)
    {
        var resource = service.StartResource("metaparse_test");

        resource.Should().BeOfType<MixedResource>();
        var mixedResource = (MixedResource)resource!;

        mixedResource.IncludedResources.Should().ContainSingle();
        mixedResource.IncludedResources[0].ResourceName.Should().Be("getresourceinfo_test");
        mixedResource.IncludedResources[0].MinimumVersion.Should().Be("1.0");
        mixedResource.IncludedResources[0].MaximumVersion.Should().Be("2.0");

        mixedResource.Maps.Should().ContainSingle();
        mixedResource.Maps[0].Source.Should().Be("maps/test.map");
        mixedResource.Maps[0].Dimension.Should().Be(7);

        mixedResource.HtmlFiles.Should().ContainSingle();
        mixedResource.HtmlFiles[0].Source.Should().Be("web/index.html");
        mixedResource.HtmlFiles[0].IsDefault.Should().BeTrue();
        mixedResource.HtmlFiles[0].IsRaw.Should().BeFalse();

        mixedResource.MinimumMtaVersion.Should().NotBeNull();
        mixedResource.MinimumMtaVersion!.Value.Client.Should().Be("1.6.0-9.22279.0");
        mixedResource.MinimumMtaVersion!.Value.Server.Should().Be("1.6.0-9.22279.0");

        mixedResource.AclRequestRights.Should().HaveCount(2);
        mixedResource.AclRequestRights.Should().Contain(x => x.Name == "function.startResource" && x.Access);
        mixedResource.AclRequestRights.Should().Contain(x => x.Name == "function.stopResource" && !x.Access);

        mixedResource.SyncMapElementData.Should().BeFalse();
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void StartMetaParseTestResource_StartsIncludedResourceFirst(
        IDropInReplacementResourceService service)
    {
        var resource = service.StartResource("metaparse_test");

        resource.Should().NotBeNull();
        service.StartedResources.Select(x => x.Name).Should().ContainInOrder("getresourceinfo_test", "metaparse_test");
    }
}
