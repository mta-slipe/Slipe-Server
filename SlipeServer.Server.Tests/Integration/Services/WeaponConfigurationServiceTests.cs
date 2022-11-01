using Moq;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using SlipeServer.Server.TestTools;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Services;

public class WeaponConfigurationServiceTests
{
    [Fact]
    public void GetWeaponConfiguration_SetWeaponConfiguration_SendsAppropriatePacket()
    {
        var mtaServer = new TestingServer();
        var player = mtaServer.AddFakePlayer();

        var service = new WeaponConfigurationService(mtaServer);

        var original = service.GetWeaponConfiguration(WeaponId.Deagle);
        var modified = original;
        modified.MaximumClipAmmo = 15;
        service.SetWeaponConfiguration(WeaponId.Deagle, modified);

        mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA, player, new byte[]
        {
            (byte)ElementRpcFunction.SET_WEAPON_PROPERTY,
            (byte)WeaponId.Deagle,
            (byte)WeaponProperty.MaxClipAmmo,
            (byte)WeaponSkillLevel.Poor,
            15, 0
        });
    }

    [Fact]
    public void GetWeaponConfiguration_SetWeaponConfigurationFor_SendsToAppropriateClients()
    {
        var mtaServer = new TestingServer();
        var player = mtaServer.AddFakePlayer();
        var player2 = mtaServer.AddFakePlayer();

        var service = new WeaponConfigurationService(mtaServer);

        var original = service.GetWeaponConfiguration(WeaponId.Deagle);
        var modified = original;
        modified.MaximumClipAmmo = 15;
        service.SetWeaponConfigurationFor(WeaponId.Deagle, modified, player);

        mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA, player, new byte[]
        {
            (byte)ElementRpcFunction.SET_WEAPON_PROPERTY,
            (byte)WeaponId.Deagle,
            (byte)WeaponProperty.MaxClipAmmo,
            (byte)WeaponSkillLevel.Poor,
            15, 0
        });

        mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA, player2, new byte[]
        {
            (byte)ElementRpcFunction.SET_WEAPON_PROPERTY,
            (byte)WeaponId.Deagle,
            (byte)WeaponProperty.MaxClipAmmo,
            (byte)WeaponSkillLevel.Poor,
            15, 0
        }, 0);
    }
}
