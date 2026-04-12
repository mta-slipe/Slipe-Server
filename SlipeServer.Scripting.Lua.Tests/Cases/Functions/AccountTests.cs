using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Functions;

public class AccountTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void AddAccount_CreatesAccount(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("TestUser", "password123")
            assertPrint(tostring(account ~= nil))
            assertPrint(getAccountName(account))
            assertPrint(getAccountType(account))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(3);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("TestUser");
        assertDataProvider.AssertPrints[2].Should().Be("registered");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAccount_WithCorrectPassword_ReturnsAccount(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addAccount("Alice", "secret")
            local account = getAccount("Alice", "secret")
            assertPrint(tostring(account ~= nil))
            assertPrint(getAccountName(account))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("Alice");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAccount_WithWrongPassword_ReturnsNil(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addAccount("Bob", "correctpass")
            local account = getAccount("Bob", "wrongpass")
            assertPrint(tostring(account))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAccount_WithoutPassword_ReturnsAccount(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addAccount("Charlie", "pass")
            local account = getAccount("Charlie")
            assertPrint(tostring(account ~= nil))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAccountID_ReturnsPositiveID(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("IDUser", "pass")
            local id = getAccountID(account)
            assertPrint(tostring(id > 0))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAccountByID_ReturnsCorrectAccount(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("IDLookup", "pass")
            local id = getAccountID(account)
            local found = getAccountByID(id)
            assertPrint(tostring(found ~= nil))
            assertPrint(getAccountName(found))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("IDLookup");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAccounts_ReturnsAll(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addAccount("Acc1", "pass")
            addAccount("Acc2", "pass")
            addAccount("Acc3", "pass")
            local accounts = getAccounts()
            assertPrint(tostring(#accounts >= 3))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RemoveAccount_RemovesFromLookup(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("ToRemove", "pass")
            local removed = removeAccount(account)
            assertPrint(tostring(removed))
            local found = getAccount("ToRemove")
            assertPrint(tostring(found))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetAccountData_GetAccountData_RoundTrip(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("DataUser", "pass")
            setAccountData(account, "score", "100")
            setAccountData(account, "level", "5")
            assertPrint(getAccountData(account, "score"))
            assertPrint(getAccountData(account, "level"))
            assertPrint(tostring(getAccountData(account, "missing")))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(3);
        assertDataProvider.AssertPrints[0].Should().Be("100");
        assertDataProvider.AssertPrints[1].Should().Be("5");
        assertDataProvider.AssertPrints[2].Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAllAccountData_ReturnsAllData(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("AllDataUser", "pass")
            setAccountData(account, "key1", "val1")
            setAccountData(account, "key2", "val2")
            local data = getAllAccountData(account)
            assertPrint(tostring(data ~= nil))
            assertPrint(data["key1"])
            assertPrint(data["key2"])
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(3);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("val1");
        assertDataProvider.AssertPrints[2].Should().Be("val2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAccountsByData_ReturnMatchingAccounts(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local a1 = addAccount("DataSearch1", "pass")
            local a2 = addAccount("DataSearch2", "pass")
            local a3 = addAccount("DataSearch3", "pass")
            setAccountData(a1, "rank", "admin")
            setAccountData(a2, "rank", "admin")
            setAccountData(a3, "rank", "user")
            local admins = getAccountsByData("rank", "admin")
            assertPrint(tostring(#admins))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetAccountPassword_AllowsLoginWithNewPassword(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("PassChange", "oldpass")
            setAccountPassword(account, "newpass")
            local found = getAccount("PassChange", "newpass")
            assertPrint(tostring(found ~= nil))
            local notFound = getAccount("PassChange", "oldpass")
            assertPrint(tostring(notFound))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetAccountName_RenamesAccount(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("OldName", "pass")
            local ok = setAccountName(account, "NewName")
            assertPrint(tostring(ok))
            assertPrint(getAccountName(account))
            local found = getAccount("NewName")
            assertPrint(tostring(found ~= nil))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(3);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("NewName");
        assertDataProvider.AssertPrints[2].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CopyAccountData_CopiesAllEntries(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local src = addAccount("CopySrc", "pass")
            local dst = addAccount("CopyDst", "pass")
            setAccountData(src, "money", "500")
            setAccountData(src, "level", "10")
            copyAccountData(dst, src)
            assertPrint(getAccountData(dst, "money"))
            assertPrint(getAccountData(dst, "level"))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("500");
        assertDataProvider.AssertPrints[1].Should().Be("10");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsGuestAccount_ReturnsTrueForGuest(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local account = getPlayerAccount(testPlayer)
            assertPrint(tostring(isGuestAccount(account)))
            assertPrint(getAccountType(account))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("guest");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void LogIn_LogOut_SetsAndClearsPlayerAccount(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local account = addAccount("LoginUser", "mypassword")
            local loggedIn = logIn(testPlayer, account, "mypassword")
            assertPrint(tostring(loggedIn))

            local currentAccount = getPlayerAccount(testPlayer)
            assertPrint(tostring(isGuestAccount(currentAccount)))
            assertPrint(getAccountName(currentAccount))

            local loggedOut = logOut(testPlayer)
            assertPrint(tostring(loggedOut))

            local afterLogout = getPlayerAccount(testPlayer)
            assertPrint(tostring(isGuestAccount(afterLogout)))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(5);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("false");
        assertDataProvider.AssertPrints[2].Should().Be("LoginUser");
        assertDataProvider.AssertPrints[3].Should().Be("true");
        assertDataProvider.AssertPrints[4].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void LogIn_WrongPassword_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local account = addAccount("WrongPassUser", "correct")
            local result = logIn(testPlayer, account, "wrong")
            assertPrint(tostring(result))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAccountPlayer_ReturnsLoggedInPlayer(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local account = addAccount("PlayerLookup", "pass")
            logIn(testPlayer, account, "pass")
            local foundPlayer = getAccountPlayer(account)
            assertPrint(tostring(foundPlayer == testPlayer))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GuestAccount_SetGetData_WorksInMemory(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local account = getPlayerAccount(testPlayer)
            assertPrint(tostring(isGuestAccount(account)))
            setAccountData(account, "temp", "value123")
            assertPrint(getAccountData(account, "temp"))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("value123");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetAccountData_TypedValues_RoundTrip(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("TypedUser", "pass")
            setAccountData(account, "score",   100)
            setAccountData(account, "ratio",   3.14)
            setAccountData(account, "active",  true)
            setAccountData(account, "name",    "hero")

            assertPrint(type(getAccountData(account, "score")))
            assertPrint(type(getAccountData(account, "ratio")))
            assertPrint(type(getAccountData(account, "active")))
            assertPrint(type(getAccountData(account, "name")))

            assertPrint(tostring(getAccountData(account, "score")))
            assertPrint(tostring(getAccountData(account, "active")))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(6);
        assertDataProvider.AssertPrints[0].Should().Be("number");
        assertDataProvider.AssertPrints[1].Should().Be("number");
        assertDataProvider.AssertPrints[2].Should().Be("boolean");
        assertDataProvider.AssertPrints[3].Should().Be("string");
        assertDataProvider.AssertPrints[4].Should().Be("100");
        assertDataProvider.AssertPrints[5].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetAccountData_NilAndFalseBool_DeleteData(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local account = addAccount("DeleteUser", "pass")
            setAccountData(account, "toNil",   "exists")
            setAccountData(account, "toBool",  "exists")
            setAccountData(account, "toNil",   nil)
            setAccountData(account, "toBool",  false)
            assertPrint(tostring(getAccountData(account, "toNil")))
            assertPrint(tostring(getAccountData(account, "toBool")))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("nil");
        assertDataProvider.AssertPrints[1].Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GuestAccount_TypedData_WorksInMemory(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local account = getPlayerAccount(testPlayer)
            setAccountData(account, "level", 42)
            setAccountData(account, "flag",  true)
            assertPrint(type(getAccountData(account, "level")))
            assertPrint(type(getAccountData(account, "flag")))
            assertPrint(tostring(getAccountData(account, "level")))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(3);
        assertDataProvider.AssertPrints[0].Should().Be("number");
        assertDataProvider.AssertPrints[1].Should().Be("boolean");
        assertDataProvider.AssertPrints[2].Should().Be("42");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAccountsBySerial_ReturnsAccountsWithSerial(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local account = addAccount("SerialUser", "pass")
            logIn(testPlayer, account, "pass")
            local serial = getPlayerSerial(testPlayer)
            local accounts = getAccountsBySerial(serial)
            assertPrint(tostring(#accounts >= 1))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }
}
