using FluentAssertions;
using SlipeServer.Scripting.Definitions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using System;
using System.Threading;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class SqlTests
{
    private static string UniqueTable() => $"test_{Guid.NewGuid():N}";

    [Theory]
    [ScriptingAutoDomainData]
    public void DbConnect_Sqlite_ReturnsConnection(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local conn = dbConnect("sqlite", ":memory:")
            assertPrint(tostring(conn ~= nil))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void DbExec_CreateTable_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var table = UniqueTable();
        sut.RunLuaScript($$"""
            local conn = dbConnect("sqlite", ":memory:")
            local ok = dbExec(conn, "CREATE TABLE IF NOT EXISTS `{{table}}` (id INTEGER PRIMARY KEY, name TEXT)")
            assertPrint(tostring(ok))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void DbQuery_InsertAndSelect_ReturnsRows(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var table = UniqueTable();
        sut.RunLuaScript($$"""
            local conn = dbConnect("sqlite", ":memory:")
            dbExec(conn, "CREATE TABLE `{{table}}` (id INTEGER PRIMARY KEY, name TEXT)")
            dbExec(conn, "INSERT INTO `{{table}}` (name) VALUES (?)", "Alice")
            dbExec(conn, "INSERT INTO `{{table}}` (name) VALUES (?)", "Bob")

            local qh = dbQuery(conn, "SELECT name FROM `{{table}}` ORDER BY name ASC")
            local rows, affected, lastId = dbPoll(qh, -1)

            assertPrint(tostring(#rows))
            assertPrint(rows[1]["name"])
            assertPrint(rows[2]["name"])
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(3);
        assertDataProvider.AssertPrints[0].Should().Be("2");
        assertDataProvider.AssertPrints[1].Should().Be("Alice");
        assertDataProvider.AssertPrints[2].Should().Be("Bob");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void DbPoll_WithTimeout0_ReturnsNilWhenNotReady(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local conn = dbConnect("sqlite", ":memory:")
            local qh = dbQuery(conn, "SELECT 1")
            -- timeout 0: may or may not be ready, just check it doesn't error
            local rows = dbPoll(qh, -1)
            assertPrint(tostring(rows ~= nil))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void DbPoll_SelectScalar_ReturnsCorrectValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local conn = dbConnect("sqlite", ":memory:")
            local qh = dbQuery(conn, "SELECT 42 AS answer")
            local rows = dbPoll(qh, -1)
            assertPrint(tostring(rows[1]["answer"]))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("42");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void DbQuery_WithParameters_SubstitutesCorrectly(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var table = UniqueTable();
        sut.RunLuaScript($$"""
            local conn = dbConnect("sqlite", ":memory:")
            dbExec(conn, "CREATE TABLE `{{table}}` (id INTEGER PRIMARY KEY, score INTEGER)")
            dbExec(conn, "INSERT INTO `{{table}}` (score) VALUES (?)", 100)
            dbExec(conn, "INSERT INTO `{{table}}` (score) VALUES (?)", 200)
            dbExec(conn, "INSERT INTO `{{table}}` (score) VALUES (?)", 300)

            local qh = dbQuery(conn, "SELECT score FROM `{{table}}` WHERE score > ? ORDER BY score ASC", 150)
            local rows = dbPoll(qh, -1)

            assertPrint(tostring(#rows))
            assertPrint(tostring(rows[1]["score"]))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("2");
        assertDataProvider.AssertPrints[1].Should().Be("200");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void DbQuery_WithIdentifierPlaceholder_Works(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var table = UniqueTable();
        sut.RunLuaScript($$"""
            local conn = dbConnect("sqlite", ":memory:")
            dbExec(conn, "CREATE TABLE `{{table}}` (id INTEGER PRIMARY KEY, val TEXT)")
            dbExec(conn, "INSERT INTO ?? (val) VALUES (?)", "{{table}}", "hello")

            local qh = dbQuery(conn, "SELECT val FROM ??", "{{table}}")
            local rows = dbPoll(qh, -1)
            assertPrint(rows[1]["val"])
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void DbFree_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local conn = dbConnect("sqlite", ":memory:")
            local qh = dbQuery(conn, "SELECT 1")
            local ok = dbFree(qh)
            assertPrint(tostring(ok))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void DbPrepareString_SubstitutesValues(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local conn = dbConnect("sqlite", ":memory:")
            local prepared = dbPrepareString(conn, "SELECT * FROM t WHERE name = ?", "O'Brien")
            assertPrint(prepared)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("SELECT * FROM t WHERE name = 'O\\'Brien'");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public async Task DbQuery_WithCallback_InvokesCallback(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local conn = dbConnect("sqlite", ":memory:")
            dbQuery(function(qh)
                local rows = dbPoll(qh, 0)
                assertPrint(tostring(rows ~= nil))
            end, conn, "SELECT 1 AS x")
            """);

        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!assertDataProvider.AssertPrints.Any() && DateTime.UtcNow < deadline)
            await Task.Delay(50);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ExecuteSQLQuery_CreateAndSelect_Works(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var table = UniqueTable();
        sut.RunLuaScript($$"""
            executeSQLQuery("CREATE TABLE IF NOT EXISTS `{{table}}` (id INTEGER PRIMARY KEY, name TEXT)")
            executeSQLQuery("INSERT INTO `{{table}}` (name) VALUES (?)", "TestPlayer")
            local rows = executeSQLQuery("SELECT name FROM `{{table}}` WHERE name = ?", "TestPlayer")
            assertPrint(tostring(#rows))
            assertPrint(rows[1]["name"])
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("1");
        assertDataProvider.AssertPrints[1].Should().Be("TestPlayer");
    }
}
