using FluentAssertions;
using SlipeServer.Scripting.Definitions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class AclTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void AclCreate_ReturnsNonNilObject(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclCreate("MyACL")
            assertPrint(tostring(acl ~= nil))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclGetName_ReturnsCorrectName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclCreate("TestACL")
            assertPrint(aclGetName(acl))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("TestACL");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclGet_ExistingAcl_ReturnsObject(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            aclCreate("Admin")
            local acl = aclGet("Admin")
            assertPrint(tostring(acl ~= nil))
            assertPrint(aclGetName(acl))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("Admin");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclGet_NonExistent_ReturnsNil(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("DoesNotExist")
            assertPrint(tostring(acl))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclGetRight_AlwaysReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclCreate("TestACL")
            assertPrint(tostring(aclGetRight(acl, "command.kick")))
            assertPrint(tostring(aclGetRight(acl, "function.banPlayer")))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclCreateGroup_ReturnsNonNilObject(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclCreateGroup("Admin")
            assertPrint(tostring(group ~= nil))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclGroupGetName_ReturnsCorrectName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclCreateGroup("Moderator")
            assertPrint(aclGroupGetName(group))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("Moderator");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclGetGroup_ExistingGroup_ReturnsObject(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            aclCreateGroup("Admin")
            local group = aclGetGroup("Admin")
            assertPrint(tostring(group ~= nil))
            assertPrint(aclGroupGetName(group))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("Admin");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclGroupList_ReturnsCreatedGroups(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            aclCreateGroup("Admin")
            aclCreateGroup("Moderator")
            local groups = aclGroupList()
            assertPrint(tostring(#groups))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclList_ReturnsCreatedAcls(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            aclCreate("Default")
            aclCreate("Admin")
            local acls = aclList()
            assertPrint(tostring(#acls))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void HasObjectPermissionTo_AlwaysReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(hasObjectPermissionTo("user.admin", "function.kickPlayer")))
            assertPrint(tostring(hasObjectPermissionTo("resource.myresource", "command.ban", false)))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsObjectInACLGroup_AlwaysReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclCreateGroup("Admin")
            assertPrint(tostring(isObjectInACLGroup("user.admin", group)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclDestroy_RemovesAcl(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclCreate("Temporary")
            local result = aclDestroy(acl)
            assertPrint(tostring(result))
            assertPrint(tostring(aclGet("Temporary")))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclSave_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(aclSave()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AclSetRight_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclCreate("TestACL")
            assertPrint(tostring(aclSetRight(acl, "command.kick", true)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }
}
