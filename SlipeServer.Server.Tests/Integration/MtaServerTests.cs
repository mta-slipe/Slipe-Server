using FluentAssertions;
using SlipeServer.Server.Resources;
using SlipeServer.Server.TestTools;
using System;
using Xunit;

namespace SlipeServer.Server.Tests.Integration;

public sealed class MtaServerTests
{
    public class TestResource : Resource
    {
        public TestResource(MtaServer server) : base(server, server.RootElement, "test", null) { }
    }

    [Fact]
    public void AddAdditionalResource_ShouldAllowAddingUniqueResourceType_AndThrowIfDuplicate()
    {
        var mtaServer = new TestingServer();

        var act = () => mtaServer.AddAdditionalResource(new TestResource(mtaServer), []);

        act.Should().NotThrow();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"A resource of type '{typeof(TestResource).Name}' has already been added.");

        var resource = mtaServer.GetAdditionalResource<TestResource>();
        resource.Should().NotBeNull();
    }
}
