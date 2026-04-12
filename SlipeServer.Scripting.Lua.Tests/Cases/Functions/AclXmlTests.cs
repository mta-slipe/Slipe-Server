using FluentAssertions;
using SlipeServer.Scripting.Definitions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using System.Xml.Linq;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

/// <summary>
/// Tests for ACL operations backed by a real acl.xml file (via XmlAclService).
/// The test XML has:
///   - Group "Everyone": ACL "Default" + objects user.* / resource.*
///   - Group "Admin":    ACL "Admin"   + object  user.Admin
///   - ACL "Default":  command.kick=false, function.kickPlayer=false
///   - ACL "Admin":    command.kick=true, function.kickPlayer=true, function.loadstring=true (with metadata)
/// </summary>
public class AclXmlTests
{
    #region Loading

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_LoadsDefaultAcl(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("Default")
            assertPrint(tostring(acl ~= nil))
            assertPrint(aclGetName(acl))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("Default");

        var acl = aclService.GetAcl("Default");
        acl.Should().NotBeNull();
        acl!.Name.Should().Be("Default");
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_LoadsAdminAcl(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("Admin")
            assertPrint(tostring(acl ~= nil))
            assertPrint(aclGetName(acl))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("Admin");

        var acl = aclService.GetAcl("Admin");
        acl.Should().NotBeNull();
        acl!.Name.Should().Be("Admin");
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_LoadsEveryoneGroup(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclGetGroup("Everyone")
            assertPrint(tostring(group ~= nil))
            assertPrint(aclGroupGetName(group))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("Everyone");

        var group = aclService.GetGroup("Everyone");
        group.Should().NotBeNull();
        group!.Name.Should().Be("Everyone");
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_LoadsAdminGroup(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclGetGroup("Admin")
            assertPrint(tostring(group ~= nil))
            assertPrint(aclGroupGetName(group))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("Admin");

        var group = aclService.GetGroup("Admin");
        group.Should().NotBeNull();
        group!.Name.Should().Be("Admin");
    }

    #endregion

    #region Rights

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_DefaultAcl_ModifyOtherObjectsIsFalse(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("Default")
            assertPrint(tostring(aclGetRight(acl, "general.ModifyOtherObjects")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
        aclService.GetRight(aclService.GetAcl("Default")!, "general.ModifyOtherObjects").Should().BeFalse();
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_DefaultAcl_AdminAreaIsFalse(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("Default")
            assertPrint(tostring(aclGetRight(acl, "general.AdminArea")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
        aclService.GetRight(aclService.GetAcl("Default")!, "general.AdminArea").Should().BeFalse();
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_AdminAcl_KickIsTrue(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("Admin")
            assertPrint(tostring(aclGetRight(acl, "command.kick")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        aclService.GetRight(aclService.GetAcl("Admin")!, "command.kick").Should().BeTrue();
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_AdminAcl_LoadstringIsTrue(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("Admin")
            assertPrint(tostring(aclGetRight(acl, "function.loadstring")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        aclService.GetRight(aclService.GetAcl("Admin")!, "function.loadstring").Should().BeTrue();
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_ListRights_DefaultAcl_ReturnsBothRights(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("Default")
            local rights = aclListRights(acl, "all")
            local count = 0
            for _, _ in ipairs(rights) do count = count + 1 end
            assertPrint(tostring(count))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
        aclService.ListRights(aclService.GetAcl("Default")!, "all").Should().HaveCount(2);
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_ListRights_AdminAcl_ReturnsThreeRights(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("Admin")
            local rights = aclListRights(acl, "all")
            local count = 0
            for _, _ in ipairs(rights) do count = count + 1 end
            assertPrint(tostring(count))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("3");
        aclService.ListRights(aclService.GetAcl("Admin")!, "all").Should().HaveCount(3);
    }

    #endregion

    #region Group membership

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_EveryoneGroup_ContainsDefaultAcl(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclGetGroup("Everyone")
            local acls = aclGroupListACL(group)
            local found = false
            for _, a in ipairs(acls) do
                if aclGetName(a) == "Default" then found = true end
            end
            assertPrint(tostring(found))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        aclService.GroupListAcls(aclService.GetGroup("Everyone")!)
            .Select(a => a.Name).Should().Contain("Default");
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_AdminGroup_ContainsAdminAcl(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclGetGroup("Admin")
            local acls = aclGroupListACL(group)
            local found = false
            for _, a in ipairs(acls) do
                if aclGetName(a) == "Admin" then found = true end
            end
            assertPrint(tostring(found))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        aclService.GroupListAcls(aclService.GetGroup("Admin")!)
            .Select(a => a.Name).Should().Contain("Admin");
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_EveryoneGroup_ListObjects_ReturnsWildcards(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclGetGroup("Everyone")
            local objects = aclGroupListObjects(group)
            local count = 0
            for _, _ in ipairs(objects) do count = count + 1 end
            assertPrint(tostring(count))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
        aclService.GroupListObjects(aclService.GetGroup("Everyone")!)
            .Should().HaveCount(2).And.Contain("user.*").And.Contain("resource.*");
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_AdminGroup_ListObjects_ReturnsOneObject(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclGetGroup("Admin")
            local objects = aclGroupListObjects(group)
            local count = 0
            for _, _ in ipairs(objects) do count = count + 1 end
            assertPrint(tostring(count))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
        aclService.GroupListObjects(aclService.GetGroup("Admin")!)
            .Should().ContainSingle().Which.Should().Be("user.Admin");
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_IsObjectInAclGroup_WildcardUser_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclGetGroup("Everyone")
            assertPrint(tostring(isObjectInACLGroup("user.Bob", group)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        aclService.IsObjectInAclGroup("user.Bob", aclService.GetGroup("Everyone")!).Should().BeTrue();
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_IsObjectInAclGroup_ExactAdminUser_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclGetGroup("Admin")
            assertPrint(tostring(isObjectInACLGroup("user.Admin", group)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        aclService.IsObjectInAclGroup("user.Admin", aclService.GetGroup("Admin")!).Should().BeTrue();
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_IsObjectInAclGroup_NonAdminUser_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local group = aclGetGroup("Admin")
            assertPrint(tostring(isObjectInACLGroup("user.Bob", group)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
        aclService.IsObjectInAclGroup("user.Bob", aclService.GetGroup("Admin")!).Should().BeFalse();
    }

    #endregion

    #region Permissions

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_HasObjectPermissionTo_AdminUser_Kick_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(hasObjectPermissionTo("user.Admin", "command.kick")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        aclService.HasObjectPermissionTo("user.Admin", "command.kick").Should().BeTrue();
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_HasObjectPermissionTo_RegularUser_AdminArea_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        // user.Bob is in Everyone (Default ACL: general.AdminArea=false) — deny wins
        sut.RunLuaScript("""
            assertPrint(tostring(hasObjectPermissionTo("user.Bob", "general.AdminArea")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
        aclService.HasObjectPermissionTo("user.Bob", "general.AdminArea").Should().BeFalse();
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_HasObjectPermissionTo_Resource_UnknownRight_UsesDefault(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        // Right not in any ACL — result falls back to defaultPermission
        sut.RunLuaScript("""
            assertPrint(tostring(hasObjectPermissionTo("resource.test", "general.SomeUnknownRight", false)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
        aclService.HasObjectPermissionTo("resource.test", "general.SomeUnknownRight", false).Should().BeFalse();
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_HasObjectPermissionTo_AdminUser_Loadstring_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(hasObjectPermissionTo("user.Admin", "function.loadstring")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        aclService.HasObjectPermissionTo("user.Admin", "function.loadstring").Should().BeTrue();
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_HasObjectPermissionTo_UnknownObject_UsesDefaultTrue(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(hasObjectPermissionTo("user.Unknown", "command.someAction", true)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        aclService.HasObjectPermissionTo("user.Unknown", "command.someAction", true).Should().BeTrue();
    }

    #endregion

    #region Persistence

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_Save_AndReload_PreservesRights(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        AclXmlFilePath filePath,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("Admin")
            aclSetRight(acl, "command.newcmd", true)
            aclSave()
            aclReload()
            local reloaded = aclGet("Admin")
            assertPrint(tostring(aclGetRight(reloaded, "command.newcmd")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");

        aclService.GetRight(aclService.GetAcl("Admin")!, "command.newcmd").Should().BeTrue();

        var doc = XDocument.Load(filePath.Value);
        doc.Root!.Elements("acl")
            .Single(e => (string)e.Attribute("name")! == "Admin")
            .Elements("right")
            .Should().Contain(e =>
                (string)e.Attribute("name")! == "command.newcmd" &&
                (string)e.Attribute("access")! == "true");
    }

    [Theory]
    [AclXmlAutoDomainData]
    public void AclXml_Reload_RestoresOriginalState(
        AssertDataProvider assertDataProvider,
        XmlAclService aclService,
        AclXmlFilePath filePath,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local acl = aclGet("Default")
            aclSetRight(acl, "general.ModifyOtherObjects", true)
            assertPrint(tostring(aclGetRight(acl, "general.ModifyOtherObjects")))
            aclReload()
            local reloaded = aclGet("Default")
            assertPrint(tostring(aclGetRight(reloaded, "general.ModifyOtherObjects")))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("false");

        aclService.GetRight(aclService.GetAcl("Default")!, "general.ModifyOtherObjects").Should().BeFalse();

        var doc = XDocument.Load(filePath.Value);
        doc.Root!.Elements("acl")
            .Single(e => (string)e.Attribute("name")! == "Default")
            .Elements("right")
            .Should().Contain(e =>
                (string)e.Attribute("name")! == "general.ModifyOtherObjects" &&
                (string)e.Attribute("access")! == "false");
    }

    #endregion
}
