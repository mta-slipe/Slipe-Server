using SlipeServer.Packets.Enums;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Functions;

public class AudioTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void PlaySoundFrontEnd_ToPlayer_SendsPacketToPlayer(
        LightTestPlayer player,
        TestPacketContext context,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);
        context.ResetPacketCountVerification();

        sut.RunLuaScript("""
            local result = playSoundFrontEnd(testPlayer, 44)
            assert(result == true)
            """);

        context.VerifyPacketSent(PacketId.PACKET_ID_LUA, player);
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void PlaySoundFrontEnd_ToRoot_SendsPacketToAllPlayers(
        LightTestPlayer player1,
        LightTestPlayer player2,
        TestPacketContext context,
        IMtaServer sut)
    {
        context.ResetPacketCountVerification();

        sut.RunLuaScript("""
            local result = playSoundFrontEnd(root, 44)
            assert(result == true)
            """);

        context.VerifyPacketSent(PacketId.PACKET_ID_LUA, player1);
        context.VerifyPacketSent(PacketId.PACKET_ID_LUA, player2);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void PlaySoundFrontEnd_ToPlayerTable_SendsPacketToEachPlayer(
        LightTestPlayer player1,
        LightTestPlayer player2,
        TestPacketContext context,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer1", player1);
        sut.AddGlobal("testPlayer2", player2);
        context.ResetPacketCountVerification();

        sut.RunLuaScript("""
            local result = playSoundFrontEnd({testPlayer1, testPlayer2}, 44)
            assert(result == true)
            """);

        context.VerifyPacketSent(PacketId.PACKET_ID_LUA, player1);
        context.VerifyPacketSent(PacketId.PACKET_ID_LUA, player2);
    }
}
