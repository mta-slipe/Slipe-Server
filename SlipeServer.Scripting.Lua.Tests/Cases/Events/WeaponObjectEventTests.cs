using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class WeaponObjectEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnWeaponFire_FiresWhenWeaponObjectFires(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var weapon = new WeaponObject(model: 355, Vector3.Zero).AssociateWith(sut);

        sut.AddGlobal("testWeapon", weapon);

        sut.RunLuaScript("""
            addEventHandler("onWeaponFire", testWeapon, function()
                assertPrint("fired")
            end)
            """);

        weapon.TriggerFired();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("fired");
    }
}
