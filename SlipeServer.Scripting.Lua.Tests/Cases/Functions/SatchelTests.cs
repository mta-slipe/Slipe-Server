using FluentAssertions;
using SlipeServer.Packets.Enums;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class SatchelTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void DetonateSatchels_ReturnsTrueAndSendsPacketToPlayer(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        TestPacketContext context,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local result = detonateSatchels(testPlayer)
            assertPrint(tostring(result))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        context.GetPacketCount(PacketId.PACKET_ID_DETONATE_SATCHELS, player).Should().Be(1);
    }
}
