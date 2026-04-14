using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class AccountEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnAccountCreate_FiresWhenAccountIsAdded(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEventHandler("onAccountCreate", getRootElement(), function(account)
                assertPrint(getAccountName(account))
            end)
            addAccount("EventUser", "pass")
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("EventUser");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnAccountRemove_FiresWhenAccountIsRemoved(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEventHandler("onAccountRemove", getRootElement(), function(account)
                assertPrint(getAccountName(account))
            end)
            local account = addAccount("RemoveEventUser", "pass")
            removeAccount(account)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("RemoveEventUser");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnAccountRemove_DoesNotFire_WhenAccountNotFound(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEventHandler("onAccountRemove", getRootElement(), function(account)
                assertPrint("removed")
            end)
            local account = addAccount("NoRemoveUser", "pass")
            removeAccount(account)
            removeAccount(account)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("removed");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnAccountDataChange_FiresWithCorrectKeyAndValues(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEventHandler("onAccountDataChange", getRootElement(), function(account, key, newValue, oldValue)
                assertPrint(getAccountName(account))
                assertPrint(key)
                assertPrint(tostring(newValue))
                assertPrint(tostring(oldValue))
            end)
            local account = addAccount("DataEventUser", "pass")
            setAccountData(account, "score", 42)
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(4);
        assertDataProvider.AssertPrints[0].Should().Be("DataEventUser");
        assertDataProvider.AssertPrints[1].Should().Be("score");
        assertDataProvider.AssertPrints[2].Should().Be("42");
        assertDataProvider.AssertPrints[3].Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnAccountDataChange_FiresWithOldValue_WhenOverwriting(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("OldValueUser", "pass")
            setAccountData(account, "score", 10)
            addEventHandler("onAccountDataChange", getRootElement(), function(account, key, newValue, oldValue)
                assertPrint(tostring(newValue))
                assertPrint(tostring(oldValue))
            end)
            setAccountData(account, "score", 99)
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("99");
        assertDataProvider.AssertPrints[1].Should().Be("10");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnAccountCreate_FiresOnce_PerAccount(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEventHandler("onAccountCreate", getRootElement(), function(account)
                assertPrint(getAccountName(account))
            end)
            addAccount("First", "pass")
            addAccount("Second", "pass")
            addAccount("Third", "pass")
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(3);
        assertDataProvider.AssertPrints[0].Should().Be("First");
        assertDataProvider.AssertPrints[1].Should().Be("Second");
        assertDataProvider.AssertPrints[2].Should().Be("Third");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerLogin_FiresWhenPlayerLogsIn(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addAccount("LoginUser", "pass")
            addEventHandler("onPlayerLogin", testPlayer, function(previousAccount, currentAccount)
                assertPrint(getAccountName(currentAccount))
            end)
            local account = getAccount("LoginUser")
            logIn(testPlayer, account, "pass")
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("LoginUser");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerLogout_FiresWhenPlayerLogsOut(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addAccount("LogoutUser", "pass")
            local account = getAccount("LogoutUser")
            logIn(testPlayer, account, "pass")
            addEventHandler("onPlayerLogout", testPlayer, function(previousAccount, currentAccount)
                assertPrint(getAccountName(previousAccount))
            end)
            logOut(testPlayer)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("LogoutUser");
    }
}
