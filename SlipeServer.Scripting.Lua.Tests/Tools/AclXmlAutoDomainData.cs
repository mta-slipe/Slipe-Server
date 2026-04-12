using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Scripting.Definitions;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Tools;

public record AclXmlFilePath(string Value);

public class AclXmlAutoDomainData : AutoDataAttribute
{
    public static readonly string TestAclXml = """
        <acl>
          <group name="Everyone">
            <acl name="Default"></acl>
            <object name="user.*"></object>
            <object name="resource.*"></object>
          </group>
          <group name="Admin">
            <acl name="Admin"></acl>
            <object name="user.Admin"></object>
          </group>
          <acl name="Default">
            <right name="general.ModifyOtherObjects" access="false"></right>
            <right name="general.AdminArea" access="false"></right>
          </acl>
          <acl name="Admin">
            <right name="command.kick" access="true"></right>
            <right name="function.kickPlayer" access="true"></right>
            <right name="function.loadstring" access="true" who="Console" pending="false" date="2026-01-01 00:00:00"></right>
          </acl>
        </acl>
        """;

    public AclXmlAutoDomainData() : base(CreateFixture()) { }

    public static (IFixture fixture, string aclFilePath) CreateFixtureWithPath()
    {
        var aclFilePath = Path.GetTempFileName();
        File.WriteAllText(aclFilePath, TestAclXml);
        var aclService = new XmlAclService(aclFilePath);

        var fixture = new Fixture();

        var throwing = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().FirstOrDefault();
        if (throwing != null)
            fixture.Behaviors.Remove(throwing);

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true, GenerateDelegates = true });

        var elementCollectionMock = new Mock<IElementCollection>();
        var elementCollection = elementCollectionMock.Object;

        var rootMock = new Mock<IRootElement>();

        var assertDataProvider = new AssertDataProvider();
        var customDefinitions = new ScriptingAssertDefinitions(assertDataProvider);

        var wrapper = new LightTestNetWrapper();
        var textItemService = new TextItemService();
        var server = new ScriptingTestMtaServer(wrapper, elementCollection, customDefinitions, textItemService, aclService);
        var context = new TestPacketContext(wrapper);

        fixture.Register<INetWrapper>(() => wrapper);
        fixture.Register<IMtaServer<LightTestPlayer>>(() => server);
        fixture.Register<IMtaServer>(() => server);
        fixture.Register<ScriptingTestMtaServer>(() => server);
        fixture.Register<Player>(server.CreatePlayer);
        fixture.Register<LightTestPlayer>(server.CreatePlayer);
        fixture.Register<TestPacketContext>(() => context);
        fixture.Register<IElementCollection>(() => elementCollection);
        fixture.Register<Mock<IElementCollection>>(() => elementCollectionMock);
        fixture.Register<IRootElement>(() => rootMock.Object);
        fixture.Register<Mock<IRootElement>>(() => rootMock);
        fixture.Register<ITextItemService>(() => textItemService);
        fixture.Register<TextItemService>(() => textItemService);
        fixture.Register<AssertDataProvider>(() => assertDataProvider);
        fixture.Register<IAclService>(() => aclService);
        fixture.Register<XmlAclService>(() => aclService);
        fixture.Register<AclXmlFilePath>(() => new AclXmlFilePath(aclFilePath));

        var configuration = new Configuration
        {
            LatentBandwidthLimit = 10000,
            LatentSendInterval = 100
        };
        fixture.Register<Configuration>(() => configuration);

        var timer = new TestTimerService();
        fixture.Register<ITimerService>(() => timer);
        fixture.Register<TestTimerService>(() => timer);

        return (fixture, aclFilePath);
    }

    private static IFixture CreateFixture() => CreateFixtureWithPath().fixture;
}
