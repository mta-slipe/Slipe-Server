using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System.Drawing;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class TeamTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void CreateTeam_CreatesTeam(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Team> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("createTeam('TestTeam')");

        captures.Should().ContainSingle();
        captures[0].TeamName.Should().Be("TestTeam");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateTeam_WithColor_SetsColor(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Team> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("createTeam('RedTeam', 255, 0, 0)");

        captures.Should().ContainSingle();
        captures[0].Color.R.Should().Be(255);
        captures[0].Color.G.Should().Be(0);
        captures[0].Color.B.Should().Be(0);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetTeamName_ReturnsName(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Team>()));

        var team = new Team("Warriors", Color.Red).AssociateWith(sut);
        sut.AddGlobal("testTeam", team);

        sut.RunLuaScript("""
            assertPrint(getTeamName(testTeam))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("Warriors");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTeamName_ChangesName(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Team>()));

        var team = new Team("OldName", Color.Red).AssociateWith(sut);
        sut.AddGlobal("testTeam", team);

        sut.RunLuaScript("setTeamName(testTeam, 'NewName')");

        team.TeamName.Should().Be("NewName");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetTeamColor_ReturnsColor(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Team>()));

        var team = new Team("MyTeam", Color.FromArgb(255, 100, 150, 200)).AssociateWith(sut);
        sut.AddGlobal("testTeam", team);

        sut.RunLuaScript("""
            local r, g, b = getTeamColor(testTeam)
            assertPrint(r .. "," .. g .. "," .. b)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("100,150,200");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTeamColor_ChangesColor(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Team>()));

        var team = new Team("MyTeam", Color.White).AssociateWith(sut);
        sut.AddGlobal("testTeam", team);

        sut.RunLuaScript("setTeamColor(testTeam, 50, 100, 150)");

        team.Color.R.Should().Be(50);
        team.Color.G.Should().Be(100);
        team.Color.B.Should().Be(150);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetTeamFriendlyFire_ReturnsFalseByDefault(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Team>()));

        var team = new Team("MyTeam", Color.Red).AssociateWith(sut);
        sut.AddGlobal("testTeam", team);

        sut.RunLuaScript("""
            assertPrint(tostring(getTeamFriendlyFire(testTeam)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTeamFriendlyFire_EnablesFriendlyFire(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Team>()));

        var team = new Team("MyTeam", Color.Red).AssociateWith(sut);
        sut.AddGlobal("testTeam", team);

        sut.RunLuaScript("setTeamFriendlyFire(testTeam, true)");

        team.IsFriendlyFireEnabled.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void GetTeamFromName_ReturnsCorrectTeam(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var team = new Team("FindMe", Color.Blue).AssociateWith(sut);
        sut.AddGlobal("_unused", team);

        sut.RunLuaScript("""
            local found = getTeamFromName("FindMe")
            assertPrint(getTeamName(found))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("FindMe");
    }
}
