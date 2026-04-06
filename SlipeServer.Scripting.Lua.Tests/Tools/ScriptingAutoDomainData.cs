using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.ElementCollections.Concurrent;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Tools;

public class ScriptingAutoDomainData(bool mockElementCollection = true) : AutoDataAttribute(CreateFixture(mockElementCollection))
{
    private static IFixture CreateFixture(bool mockElementCollection)
    {
        var fixture = new Fixture();

        var throwing = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().FirstOrDefault();
        if (throwing != null)
            fixture.Behaviors.Remove(throwing);

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true, GenerateDelegates = true });

        var elementCollectionMock = new Mock<IElementCollection>();
        var elementCollection = elementCollectionMock.Object;

        if (!mockElementCollection)
            elementCollection = new SpatialHashCompoundConcurrentElementCollection();

        var rootMock = new Mock<IRootElement>();

        var assertDataProvider = new AssertDataProvider();
        var customDefinitions = new ScriptingAssertDefinitions(assertDataProvider);

        var wrapper = new LightTestNetWrapper();
        var server = new ScriptingTestMtaServer(wrapper, elementCollection, customDefinitions);
        var context = new TestPacketContext(wrapper);


        fixture.Register<INetWrapper>(() => wrapper);
        fixture.Register<IMtaServer<LightTestPlayer>>(() => server);
        fixture.Register<IMtaServer>(() => server);
        fixture.Register<ScriptingTestMtaServer>(() => server);
        fixture.Register<Player>(server.CreatePlayer);
        fixture.Register<LightTestPlayer>(server.CreatePlayer);
        fixture.Register<TestPacketContext>(() => context);
        fixture.Register<IElementCollection>(() => elementCollection);

        if (mockElementCollection)
            fixture.Register<Mock<IElementCollection>>(() => elementCollectionMock);

        fixture.Register<IRootElement>(() => rootMock.Object);
        fixture.Register<Mock<IRootElement>>(() => rootMock);

        fixture.Register<AssertDataProvider>(() => assertDataProvider);

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

public class InlineAutoDomainDataAttribute(params object[] values) : InlineAutoDataAttribute(new ScriptingAutoDomainData(), values)
{
}
