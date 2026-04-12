using SlipeServer.Packets.Definitions.Lua;
using System.Collections.Generic;

namespace SlipeServer.Scripting.Definitions;

public class AclEntry(string name)
{
    public string Name { get; } = name;
    internal Dictionary<string, bool> Rights { get; } = new(System.StringComparer.OrdinalIgnoreCase);
}

public class AclGroup(string name)
{
    public string Name { get; } = name;
    internal List<AclEntry> Acls { get; } = [];
    internal HashSet<string> Objects { get; } = new(System.StringComparer.OrdinalIgnoreCase);
}

public class AclScriptDefinitions(IAclService aclService)
{
    [ScriptFunctionDefinition("aclCreate")]
    public AclEntry AclCreate(string aclName) => aclService.CreateAcl(aclName);

    [ScriptFunctionDefinition("aclCreateGroup")]
    public AclGroup AclCreateGroup(string groupName) => aclService.CreateGroup(groupName);

    [ScriptFunctionDefinition("aclDestroy")]
    public bool AclDestroy(AclEntry acl) => aclService.DestroyAcl(acl);

    [ScriptFunctionDefinition("aclDestroyGroup")]
    public bool AclDestroyGroup(AclGroup aclGroup) => aclService.DestroyGroup(aclGroup);

    [ScriptFunctionDefinition("aclGet")]
    public AclEntry? AclGet(string aclName) => aclService.GetAcl(aclName);

    [ScriptFunctionDefinition("aclGetGroup")]
    public AclGroup? AclGetGroup(string groupName) => aclService.GetGroup(groupName);

    [ScriptFunctionDefinition("aclGetName")]
    public string AclGetName(AclEntry acl) => acl.Name;

    [ScriptFunctionDefinition("aclGetRight")]
    public bool AclGetRight(AclEntry acl, string rightName) => aclService.GetRight(acl, rightName);

    [ScriptFunctionDefinition("aclGroupAddACL")]
    public bool AclGroupAddACL(AclGroup group, AclEntry acl) => aclService.GroupAddAcl(group, acl);

    [ScriptFunctionDefinition("aclGroupAddObject")]
    public bool AclGroupAddObject(AclGroup group, string objectName) => aclService.GroupAddObject(group, objectName);

    [ScriptFunctionDefinition("aclGroupGetName")]
    public string AclGroupGetName(AclGroup aclGroup) => aclGroup.Name;

    [ScriptFunctionDefinition("aclGroupList")]
    public List<AclGroup> AclGroupList() => [.. aclService.ListGroups()];

    [ScriptFunctionDefinition("aclGroupListACL")]
    public List<AclEntry> AclGroupListACL(AclGroup group) => [.. aclService.GroupListAcls(group)];

    [ScriptFunctionDefinition("aclGroupListObjects")]
    public List<string> AclGroupListObjects(AclGroup group) => [.. aclService.GroupListObjects(group)];

    [ScriptFunctionDefinition("aclGroupRemoveACL")]
    public bool AclGroupRemoveACL(AclGroup group, AclEntry acl) => aclService.GroupRemoveAcl(group, acl);

    [ScriptFunctionDefinition("aclGroupRemoveObject")]
    public bool AclGroupRemoveObject(AclGroup group, string objectName) => aclService.GroupRemoveObject(group, objectName);

    [ScriptFunctionDefinition("aclObjectGetGroups")]
    public List<AclGroup> AclObjectGetGroups(string objectName) => [.. aclService.GetGroupsForObject(objectName)];

    [ScriptFunctionDefinition("aclList")]
    public List<AclEntry> AclList() => [.. aclService.ListAcls()];

    [ScriptFunctionDefinition("aclListRights")]
    public List<string> AclListRights(AclEntry acl, string? allowedType = null) => [.. aclService.ListRights(acl, allowedType)];

    [ScriptFunctionDefinition("aclReload")]
    public bool AclReload() => aclService.Reload();

    [ScriptFunctionDefinition("aclRemoveRight")]
    public bool AclRemoveRight(AclEntry acl, string rightName) => aclService.RemoveRight(acl, rightName);

    [ScriptFunctionDefinition("aclSave")]
    public bool AclSave() => aclService.Save();

    [ScriptFunctionDefinition("aclSetRight")]
    public bool AclSetRight(AclEntry acl, string rightName, bool hasAccess) => aclService.SetRight(acl, rightName, hasAccess);

    [ScriptFunctionDefinition("hasObjectPermissionTo")]
    public bool HasObjectPermissionTo(LuaValue theObject, string action, bool defaultPermission = true)
    {
        var objectName = theObject.StringValue ?? string.Empty;
        return aclService.HasObjectPermissionTo(objectName, action, defaultPermission);
    }

    [ScriptFunctionDefinition("isObjectInACLGroup")]
    public bool IsObjectInACLGroup(string objectName, AclGroup group) => aclService.IsObjectInAclGroup(objectName, group);
}
