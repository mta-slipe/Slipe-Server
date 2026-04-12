using FluentAssertions;
using SlipeServer.Scripting.Definitions;
using System.Xml.Linq;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class AclXmlRealWorldFixture : IDisposable
{
    public XmlAclService AclService { get; }
    public string OriginalPath { get; }

    public AclXmlRealWorldFixture()
    {
        OriginalPath = Path.Combine(AppContext.BaseDirectory, "TestData", "acl.xml");
        AclService = new XmlAclService(OriginalPath);
    }

    public void Dispose() { }
}

/// <summary>
/// Unit tests that parse the real production acl.xml from SlipeServer.DropInReplacement.Console.
/// Verifies structure, rights, group membership, metadata and round-trip persistence.
/// </summary>
public class AclXmlRealWorldTests : IClassFixture<AclXmlRealWorldFixture>
{
    private readonly XmlAclService _aclService;
    private readonly string _originalPath;

    public AclXmlRealWorldTests(AclXmlRealWorldFixture fixture)
    {
        _aclService = fixture.AclService;
        _originalPath = fixture.OriginalPath;
    }

    #region Structure

    [Fact]
    public void RealWorld_LoadsAllGroups()
    {
        _aclService.ListGroups().Should().HaveCount(11);
    }

    [Fact]
    public void RealWorld_LoadsAllAcls()
    {
        _aclService.ListAcls().Should().HaveCount(10);
    }

    [Fact]
    public void RealWorld_GroupNames_ContainExpected()
    {
        _aclService.ListGroups().Select(g => g.Name).Should().Contain([
            "Everyone", "Moderator", "SuperModerator", "Admin", "Console",
            "RPC", "MapEditor", "raceACLGroup", "DevGroup",
            "autoGroup_runcode", "autoGroup_debugCompanion"
        ]);
    }

    [Fact]
    public void RealWorld_AclNames_ContainExpected()
    {
        _aclService.ListAcls().Select(a => a.Name).Should().Contain([
            "Default", "Moderator", "SuperModerator", "Admin", "RPC",
            "MapEditor", "raceACL", "DevACL", "autoACL_runcode", "autoACL_debugCompanion"
        ]);
    }

    [Fact]
    public void RealWorld_XmlFileStructure_HasCorrectRootElement()
    {
        var doc = XDocument.Load(_originalPath);
        doc.Root!.Name.LocalName.Should().Be("acl");
        doc.Root.Elements("group").Should().HaveCount(11);
        doc.Root.Elements("acl").Should().HaveCount(10);
    }

    #endregion

    #region Admin group

    [Fact]
    public void RealWorld_AdminGroup_HasFourAcls()
    {
        var adminGroup = _aclService.GetGroup("Admin")!;
        _aclService.GroupListAcls(adminGroup).Select(a => a.Name)
            .Should().BeEquivalentTo(["Moderator", "SuperModerator", "Admin", "RPC"]);
    }

    [Fact]
    public void RealWorld_AdminGroup_HasFiveObjects()
    {
        var adminGroup = _aclService.GetGroup("Admin")!;
        _aclService.GroupListObjects(adminGroup).Should().HaveCount(5)
            .And.Contain("user.NanoBob")
            .And.Contain("resource.admin")
            .And.Contain("resource.admin2")
            .And.Contain("resource.webadmin")
            .And.Contain("resource.acpanel");
    }

    [Fact]
    public void RealWorld_NanoBob_IsInAdminGroup()
    {
        _aclService.IsObjectInAclGroup("user.NanoBob", _aclService.GetGroup("Admin")!).Should().BeTrue();
    }

    [Fact]
    public void RealWorld_NanoBob_IsInEveryoneGroup()
    {
        _aclService.IsObjectInAclGroup("user.NanoBob", _aclService.GetGroup("Everyone")!).Should().BeTrue();
    }

    #endregion

    #region Default ACL rights

    [Fact]
    public void RealWorld_DefaultAcl_Has162Rights()
    {
        _aclService.ListRights(_aclService.GetAcl("Default")!, "all").Should().HaveCount(162);
    }

    [Fact]
    public void RealWorld_DefaultAcl_AllRightsAreDenied()
    {
        var defaultAcl = _aclService.GetAcl("Default")!;
        foreach (var right in _aclService.ListRights(defaultAcl, "all"))
            _aclService.GetRight(defaultAcl, right).Should().BeFalse(because: $"Default ACL denies {right}");
    }

    [Fact]
    public void RealWorld_DefaultAcl_DeniesKick()
    {
        _aclService.GetRight(_aclService.GetAcl("Default")!, "command.kick").Should().BeFalse();
    }

    [Fact]
    public void RealWorld_DefaultAcl_DeniesLoadstring()
    {
        _aclService.GetRight(_aclService.GetAcl("Default")!, "function.loadstring").Should().BeFalse();
    }

    #endregion

    #region Admin ACL rights

    [Fact]
    public void RealWorld_AdminAcl_AllowsModifyOtherObjects()
    {
        _aclService.GetRight(_aclService.GetAcl("Admin")!, "general.ModifyOtherObjects").Should().BeTrue();
    }

    [Fact]
    public void RealWorld_AdminAcl_AllowsKick()
    {
        _aclService.GetRight(_aclService.GetAcl("Admin")!, "command.kick").Should().BeTrue();
    }

    [Fact]
    public void RealWorld_AdminAcl_AllowsShutdown()
    {
        _aclService.GetRight(_aclService.GetAcl("Admin")!, "command.shutdown").Should().BeTrue();
    }

    #endregion

    #region autoACL_debugCompanion metadata

    [Fact]
    public void RealWorld_AutoAclDebugCompanion_HasFiveRights()
    {
        _aclService.ListRights(_aclService.GetAcl("autoACL_debugCompanion")!, "all").Should().HaveCount(5);
    }

    [Fact]
    public void RealWorld_AutoAclDebugCompanion_LoadstringIsTrue()
    {
        _aclService.GetRight(_aclService.GetAcl("autoACL_debugCompanion")!, "function.loadstring").Should().BeTrue();
    }

    [Fact]
    public void RealWorld_AutoAclRuncode_LoadstringIsFalse()
    {
        _aclService.GetRight(_aclService.GetAcl("autoACL_runcode")!, "function.loadstring").Should().BeFalse();
    }

    [Fact]
    public void RealWorld_AutoAclDebugCompanion_Metadata_IsPreservedOnSave()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.Copy(_originalPath, tempPath, overwrite: true);
            var saveService = new XmlAclService(tempPath);
            saveService.Save();

            var doc = XDocument.Load(tempPath);
            var loadstringRight = doc.Root!
                .Elements("acl")
                .Single(e => (string)e.Attribute("name")! == "autoACL_debugCompanion")
                .Elements("right")
                .Single(e => (string)e.Attribute("name")! == "function.loadstring");

            ((string?)loadstringRight.Attribute("who")).Should().Be("Console");
            ((string?)loadstringRight.Attribute("pending")).Should().Be("false");
            ((string?)loadstringRight.Attribute("date")).Should().Be("2026-04-06 00:41:15");
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void RealWorld_AutoAclRuncode_PendingMetadata_IsPreservedOnSave()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.Copy(_originalPath, tempPath, overwrite: true);
            var saveService = new XmlAclService(tempPath);
            saveService.Save();

            var doc = XDocument.Load(tempPath);
            var loadstringRight = doc.Root!
                .Elements("acl")
                .Single(e => (string)e.Attribute("name")! == "autoACL_runcode")
                .Elements("right")
                .Single(e => (string)e.Attribute("name")! == "function.loadstring");

            ((string?)loadstringRight.Attribute("pending")).Should().Be("true");
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    #endregion

    #region Round-trip persistence

    [Fact]
    public void RealWorld_RoundTrip_SaveAndReloadPreservesGroupAndAclCount()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.Copy(_originalPath, tempPath, overwrite: true);
            var service = new XmlAclService(tempPath);
            service.Save();
            service.Reload();

            service.ListGroups().Should().HaveCount(11);
            service.ListAcls().Should().HaveCount(10);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void RealWorld_RoundTrip_AddRight_PersistsAfterSaveReload()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.Copy(_originalPath, tempPath, overwrite: true);
            var service = new XmlAclService(tempPath);
            service.SetRight(service.GetAcl("Admin")!, "command.testright", true);
            service.Save();
            service.Reload();

            service.GetRight(service.GetAcl("Admin")!, "command.testright").Should().BeTrue();
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void RealWorld_RoundTrip_DefaultAclRights_AreStillDeniedAfterSaveReload()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.Copy(_originalPath, tempPath, overwrite: true);
            var service = new XmlAclService(tempPath);
            service.Save();
            service.Reload();

            var defaultAcl = service.GetAcl("Default")!;
            service.GetRight(defaultAcl, "command.kick").Should().BeFalse();
            service.GetRight(defaultAcl, "function.loadstring").Should().BeFalse();
            service.ListRights(defaultAcl, "all").Should().HaveCount(162);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    #endregion
}
