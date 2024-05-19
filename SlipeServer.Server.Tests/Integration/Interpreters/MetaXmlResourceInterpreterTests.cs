using FluentAssertions;
using Moq;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Interpreters;

public class MetaXmlResourceInterpreterTests
{
    private readonly MetaXmlResourceInterpreter metaXmlResourceInterpreter;
    private readonly Mock<IResourceProvider> resourceProviderMock = new(MockBehavior.Strict);
    private readonly TestingServer testingServer = new();
    public MetaXmlResourceInterpreterTests()
    {
        metaXmlResourceInterpreter = new();
    }

    [Fact]
    public void TestTryInterpretResourceWithMinimalResource()
    {
        this.resourceProviderMock.Setup(x => x.GetFilesForResource("testResource")).Returns(new List<string>
        {
            "meta.xml",
            "script.lua"
        });

        var metaXmlContent = """
            <meta>
                <script src="script.lua" type="client" cache="false"/>
            </meta>
            """u8.ToArray();

        this.resourceProviderMock.Setup(x => x.GetFileContent("testResource", "meta.xml")).Returns(metaXmlContent);
        this.resourceProviderMock.Setup(x => x.GetFileContent("testResource", "script.lua")).Returns("420"u8.ToArray());

        var result = this.metaXmlResourceInterpreter.TryInterpretResource(this.testingServer, null, "testResource", ".", this.resourceProviderMock.Object, out var resource, out var serverResource);

        if (resource == null)
            throw new NullReferenceException();

        resource.Name.Should().Be("testResource");
        resource.IsOopEnabled.Should().BeFalse();
        resource.Exports.Should().BeEmpty();
        resource.Files.Should().BeEmpty();
        resource.PriorityGroup.Should().Be(0);
        resource.NoClientScripts.Should().BeEquivalentTo(new Dictionary<string, byte[]>
        {
            ["script.lua"] = "420"u8.ToArray(),
        });
    }

    [Fact]
    public void TestTryInterpretResource()
    {
        this.resourceProviderMock.Setup(x => x.GetFilesForResource("testResource")).Returns(new List<string>
        {
            "meta.xml",
            "script1.lua",
            "script2.lua",
            "logo.png"
        });

        var metaXmlContent = """
            <meta>
                <script src="script1.lua" type="client" cache="false" />
                <script src="script2.lua" type="client" cache="true" />
                <file src="logo.png" />
                <export function="helloClient" type="client"/>
                <export function="helloServer" type="server"/>
                <export function="helloShared" type="shared"/>
                <oop>true</oop>
                <download_priority_group>1234</download_priority_group>
            </meta>
            """u8.ToArray();

        this.resourceProviderMock.Setup(x => x.GetFileContent("testResource", "meta.xml")).Returns(metaXmlContent);
        this.resourceProviderMock.Setup(x => x.GetFileContent("testResource", "script1.lua")).Returns("420"u8.ToArray());
        this.resourceProviderMock.Setup(x => x.GetFileContent("testResource", "script2.lua")).Returns("1337"u8.ToArray());
        this.resourceProviderMock.Setup(x => x.GetFileContent("testResource", "logo.png")).Returns("69"u8.ToArray());

        var result = this.metaXmlResourceInterpreter.TryInterpretResource(this.testingServer, null, "testResource", ".", this.resourceProviderMock.Object, out var resource, out var serverResource);

        if (resource == null)
            throw new NullReferenceException();

        resource.Name.Should().Be("testResource");
        resource.IsOopEnabled.Should().BeTrue();
        resource.Exports.Should().BeEquivalentTo("helloClient", "helloShared");
        resource.Files.Select(x => x.Name).Should().BeEquivalentTo("logo.png", "script2.lua");
        resource.PriorityGroup.Should().Be(1234);
        resource.NoClientScripts.Should().BeEquivalentTo(new Dictionary<string, byte[]>
        {
            ["script1.lua"] = "420"u8.ToArray(),
        });
    }
}
