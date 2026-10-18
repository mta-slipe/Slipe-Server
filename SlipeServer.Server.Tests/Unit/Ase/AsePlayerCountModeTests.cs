using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Tests.Tools;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Ase;

public class AsePlayerCountModeTests
{
    private sealed class Scenario
    {
        public Configuration Configuration { get; } = new()
        {
            Port = 22003,
            MaxPlayerCount = 32,
            AseBuildNumber = 24025
        };
        public Mock<IMtaServer> Server { get; } = new(MockBehavior.Strict);
        public Mock<INetWrapper> Primary { get; } = new();
        public Mock<INetWrapper> Secondary { get; } = new();
        public AseQueryService Service { get; }

        public Scenario(AsePlayerCountMode? mode = null)
        {
            if (mode.HasValue)
                Configuration.AsePlayerCountMode = mode.Value;

            Primary.Setup(x => x.GetAsePingStatus()).Returns(new byte[] { 2, 10, 11 });
            Primary.Setup(x => x.GetAseNetRoute()).Returns(new byte[] { 1, 12 });
            Secondary.Setup(x => x.GetAsePingStatus()).Returns(new byte[] { 2, 20, 21 });
            Secondary.Setup(x => x.GetAseNetRoute()).Returns(new byte[] { 1, 22 });

            Server.SetupGet(x => x.GameType).Returns("play");
            Server.SetupGet(x => x.MapName).Returns("None");
            Server.SetupGet(x => x.HasPassword).Returns(false);
            Server.SetupGet(x => x.Uptime).Returns(TimeSpan.FromSeconds(500));
            Server.Setup(x => x.GetNetWrapper(22003)).Returns(Primary.Object);
            Server.Setup(x => x.GetNetWrapper(22004)).Returns(Secondary.Object);

            var primaryPlayer = new LightTestPlayer(Primary.Object, 1) { Name = "Primary" };
            var secondaryPlayer = new LightTestPlayer(Secondary.Object, 2) { Name = "Secondary" };
            var disconnected = new LightTestPlayer(Primary.Object, 3) { Name = "Disconnected" };
            disconnected.Client.IsConnected = false;
            var joining = new Player { Name = "Joining" };
            joining.Client = new Client(4, Primary.Object, joining);
            var fake = new Player { Name = "Fake" };
            fake.Client = new FakeClient(fake)
            {
                ConnectionState = ClientConnectionState.Joined,
                IsConnected = true
            };

            var elements = new Mock<IElementCollection>(MockBehavior.Strict);
            elements.Setup(x => x.GetByType<Player>(ElementType.Player))
                .Returns(new Player[] { primaryPlayer, secondaryPlayer, disconnected, joining, fake });
            Service = new AseQueryService(Server.Object, Configuration, elements.Object);
        }
    }

    private static byte[] ReadField(BinaryReader reader) => reader.ReadBytes(reader.ReadByte() - 1);

    private static (byte[] Metadata, string[] Names, byte Count) ReadLight(byte[] packet)
    {
        using var reader = new BinaryReader(new MemoryStream(packet));
        Assert.Equal("EYE2", Encoding.ASCII.GetString(reader.ReadBytes(4)));
        for (int i = 0; i < 4; i++)
            ReadField(reader);
        byte[] metadata = ReadField(reader);
        ReadField(reader);
        reader.ReadBytes(2);
        byte count = reader.ReadByte();
        reader.ReadByte();
        var names = new System.Collections.Generic.List<string>();
        while (reader.BaseStream.Position < reader.BaseStream.Length)
            names.Add(Encoding.UTF8.GetString(ReadField(reader)));
        return (metadata, names.ToArray(), count);
    }

    private static string[] MetadataFields(byte[] metadata) => Encoding.Latin1.GetString(metadata).Split('\0');

    [Theory]
    [InlineData(22003, "Primary", 10)]
    [InlineData(22004, "Secondary", 20)]
    public void VerifiedModeUsesOnlyQueriedWrappersPlayersAndNativeData(int port, string expectedName, int pingByte)
    {
        var scenario = new Scenario(AsePlayerCountMode.VerifiedPerWrapper);
        var response = ReadLight(scenario.Service.QueryLight((ushort)port));
        Assert.Equal(1, response.Count);
        Assert.Equal(new[] { expectedName }, response.Names);
        string[] fields = MetadataFields(response.Metadata);
        Assert.Equal("1/32", fields[1]);
        Assert.Equal("24025", fields[3]);
        Assert.Equal(new byte[] { 2, (byte)pingByte, (byte)(pingByte + 1) }, Encoding.Latin1.GetBytes(fields[4]));
    }

    [Fact]
    public void DefaultModeIncludesAllWrappersAndDoesNotAccessNativeDataOrPortLookup()
    {
        var scenario = new Scenario();
        Assert.Equal(AsePlayerCountMode.Aggregate, scenario.Configuration.AsePlayerCountMode);
        var response = ReadLight(scenario.Service.QueryLight(22999));
        Assert.Equal(2, response.Count);
        Assert.Equal(new[] { "Primary", "Secondary" }, response.Names);
        string[] fields = MetadataFields(response.Metadata);
        Assert.Equal("2/32", fields[1]);
        // Missing native fields keep the advertised total explicitly unverified.
        byte[] expected = Encoding.ASCII.GetBytes("None\0" + "2/32\0" + "9\0" + "24025\0")
            .Concat(new byte[] { 0, 0 })
            .Concat(Encoding.ASCII.GetBytes("500\0" + "40680")).ToArray();
        Assert.Equal(expected, response.Metadata);
        scenario.Server.Verify(x => x.GetNetWrapper(It.IsAny<ushort>()), Times.Never);
        scenario.Primary.Verify(x => x.GetAsePingStatus(), Times.Never);
        scenario.Primary.Verify(x => x.GetAseNetRoute(), Times.Never);
        scenario.Secondary.Verify(x => x.GetAsePingStatus(), Times.Never);
    }

    [Fact]
    public void HiddenPlayersAreNotRevealedByNativeStatus()
    {
        var scenario = new Scenario(AsePlayerCountMode.VerifiedPerWrapper);
        scenario.Service.ShowPlayers = false;
        var response = ReadLight(scenario.Service.QueryLight(22003));
        Assert.Equal(0, response.Count);
        Assert.Empty(response.Names);
        Assert.Equal("0/32", MetadataFields(response.Metadata)[1]);
        scenario.Server.Verify(x => x.GetNetWrapper(It.IsAny<ushort>()), Times.Never);
        scenario.Primary.Verify(x => x.GetAsePingStatus(), Times.Never);
    }

    [Theory]
    [InlineData(AsePlayerCountMode.VerifiedPerWrapper, 1)]
    [InlineData(AsePlayerCountMode.Aggregate, 2)]
    public void FullAndXfireUseTheSamePlayerScope(AsePlayerCountMode mode, int count)
    {
        var scenario = new Scenario(mode);
        using var reader = new BinaryReader(new MemoryStream(scenario.Service.QueryFull(22004)));
        reader.ReadBytes(4);
        for (int i = 0; i < 7; i++)
            ReadField(reader);
        Assert.Equal(count.ToString(), Encoding.ASCII.GetString(ReadField(reader)));
        if (mode == AsePlayerCountMode.VerifiedPerWrapper)
        {
            ReadField(reader);
            Assert.Equal(1, reader.ReadByte());
            Assert.Equal(0x3F, reader.ReadByte());
            Assert.Equal("Secondary", Encoding.UTF8.GetString(ReadField(reader)));
        }
        byte[] xfire = scenario.Service.QueryXFireLight(22004);
        Assert.Equal(count, xfire[^2]);
    }

    [Fact]
    public void RulesSupportConcurrentUpdatesAndReads()
    {
        var scenario = new Scenario();
        Parallel.For(0, 100, index =>
        {
            string key = index.ToString();
            scenario.Service.SetRule(key, key);
            Assert.Equal(key, scenario.Service.GetRule(key));
            scenario.Service.QueryFull(22003);
        });
        for (int index = 0; index < 100; index++)
        {
            Assert.True(scenario.Service.RemoveRule(index.ToString()));
            Assert.Null(scenario.Service.GetRule(index.ToString()));
        }
    }
}
