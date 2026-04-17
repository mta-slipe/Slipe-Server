using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Tools;

public class DropInReplacementAutoDomainData : AutoDataAttribute
{
    public DropInReplacementAutoDomainData() : base(CreateFixture) { }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        var throwing = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().FirstOrDefault();
        if (throwing != null)
            fixture.Behaviors.Remove(throwing);

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true, GenerateDelegates = true });

        var resourceDirectory = Path.Combine(AppContext.BaseDirectory, "Resources");
        var wrapper = new LightTestNetWrapper();
        var timerService = new ManualScriptTimerService();
        var server = new DropInReplacementTestingServer(wrapper, resourceDirectory, timerService);
        var context = new TestPacketContext(wrapper);

        fixture.Register(() => wrapper);
        fixture.Register(() => timerService);
        fixture.Register<IScriptTimerService>(() => timerService);
        fixture.Register(() => server);
        fixture.Register<IMtaServer>(() => server);
        fixture.Register(() => server.GetRequiredService<IDropInReplacementResourceService>());
        fixture.Register(() => server.GetRequiredService<DropInReplacementResourceService>());
        fixture.Register(() => server.GetRequiredService<IScriptEventRuntime>());
        fixture.Register(() => server.GetRequiredService<IElementCollection>());
        fixture.Register(() => server.GetRequiredService<ISettingsRegistry>());
        fixture.Register(() => context);

        return fixture;
    }
}
