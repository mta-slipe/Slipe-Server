using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System.Linq;

namespace SlipeServer.Server.Tests.Tools;

public class AutoDomainDataAttribute() : AutoDataAttribute(CreateFixture)
{
    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        var throwing = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().FirstOrDefault();
        if (throwing != null) 
            fixture.Behaviors.Remove(throwing);

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true, GenerateDelegates = true });

        var elementCollectionMock = new Mock<IElementCollection>();
        var rootMock = new Mock<IRootElement>();


        var wrapper = new LightTestNetWrapper();
        var server = new LightTestMtaServer(wrapper, elementCollectionMock.Object);
        var context = new TestPacketContext(wrapper);

        fixture.Register<INetWrapper>(() => wrapper);
        fixture.Register<IMtaServer<LightTestPlayer>>(() => server);
        fixture.Register<IMtaServer>(() => server);
        fixture.Register<LightTestMtaServer>(() => server);
        fixture.Register<Player>(server.CreatePlayer);
        fixture.Register<LightTestPlayer>(server.CreatePlayer);
        fixture.Register<TestPacketContext>(() => context);
        fixture.Register<IElementCollection>(() => elementCollectionMock.Object);
        fixture.Register<Mock<IElementCollection>>(() => elementCollectionMock);
        fixture.Register<IRootElement>(() => rootMock.Object);
        fixture.Register<Mock<IRootElement>>(() => rootMock);

        var configuration = new Configuration
        {
            LatentBandwidthLimit = 10000,
            LatentSendInterval = 100
        };
        fixture.Register<Configuration>(() => configuration);

        var timer = new TestTimerService();
        fixture.Register<ITimerService>(() => timer);
        fixture.Register<TestTimerService>(() => timer);

        return fixture;
    }
}

public class InlineAutoDomainDataAttribute(params object[] values) : InlineAutoDataAttribute(new AutoDomainDataAttribute(), values)
{
}
