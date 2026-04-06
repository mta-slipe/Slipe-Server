using SlipeServer.Packets.Definitions.Lua;
using System.Collections.Generic;

namespace SlipeServer.Scripting.Definitions;

public class AclEntry(string name)
{
    public string Name { get; } = name;
}

public class AclGroup(string name)
{
    public string Name { get; } = name;
}

public class AclScriptDefinitions
{
    private readonly Dictionary<string, AclEntry> acls = new(System.StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AclGroup> groups = new(System.StringComparer.OrdinalIgnoreCase);

    [ScriptFunctionDefinition("aclCreate")]
    public AclEntry AclCreate(string aclName)
    {
        if (!this.acls.TryGetValue(aclName, out var entry))
        {
            entry = new AclEntry(aclName);
            this.acls[aclName] = entry;
        }
        return entry;
    }

    [ScriptFunctionDefinition("aclCreateGroup")]
    public AclGroup AclCreateGroup(string groupName)
    {
        if (!this.groups.TryGetValue(groupName, out var group))
        {
            group = new AclGroup(groupName);
            this.groups[groupName] = group;
        }
        return group;
    }

    [ScriptFunctionDefinition("aclDestroy")]
    public bool AclDestroy(AclEntry acl)
    {
        this.acls.Remove(acl.Name);
        return true;
    }

    [ScriptFunctionDefinition("aclDestroyGroup")]
    public bool AclDestroyGroup(AclGroup aclGroup)
    {
        this.groups.Remove(aclGroup.Name);
        return true;
    }

    [ScriptFunctionDefinition("aclGet")]
    public AclEntry? AclGet(string aclName)
    {
        this.acls.TryGetValue(aclName, out var entry);
        return entry;
    }

    [ScriptFunctionDefinition("aclGetGroup")]
    public AclGroup? AclGetGroup(string groupName)
    {
        this.groups.TryGetValue(groupName, out var group);
        return group;
    }

    [ScriptFunctionDefinition("aclGetName")]
    public string AclGetName(AclEntry acl) => acl.Name;

    [ScriptFunctionDefinition("aclGetRight")]
    public bool AclGetRight(AclEntry acl, string rightName) => true;

    [ScriptFunctionDefinition("aclGroupAddACL")]
    public bool AclGroupAddACL(AclGroup group, AclEntry acl) => true;

    [ScriptFunctionDefinition("aclGroupAddObject")]
    public bool AclGroupAddObject(AclGroup group, string objectName) => true;

    [ScriptFunctionDefinition("aclGroupGetName")]
    public string AclGroupGetName(AclGroup aclGroup) => aclGroup.Name;

    [ScriptFunctionDefinition("aclGroupList")]
    public List<AclGroup> AclGroupList() => [.. this.groups.Values];

    [ScriptFunctionDefinition("aclGroupListACL")]
    public List<AclEntry> AclGroupListACL(AclGroup group) => [];

    [ScriptFunctionDefinition("aclGroupListObjects")]
    public List<string> AclGroupListObjects(AclGroup group) => [];

    [ScriptFunctionDefinition("aclGroupRemoveACL")]
    public bool AclGroupRemoveACL(AclGroup group, AclEntry acl) => true;

    [ScriptFunctionDefinition("aclGroupRemoveObject")]
    public bool AclGroupRemoveObject(AclGroup group, string objectName) => true;

    [ScriptFunctionDefinition("aclObjectGetGroups")]
    public List<AclGroup> AclObjectGetGroups(string objectName) => [.. this.groups.Values];

    [ScriptFunctionDefinition("aclList")]
    public List<AclEntry> AclList() => [.. this.acls.Values];

    [ScriptFunctionDefinition("aclListRights")]
    public List<string> AclListRights(AclEntry acl, string? allowedType = null) => [];

    [ScriptFunctionDefinition("aclReload")]
    public bool AclReload() => true;

    [ScriptFunctionDefinition("aclRemoveRight")]
    public bool AclRemoveRight(AclEntry acl, string rightName) => true;

    [ScriptFunctionDefinition("aclSave")]
    public bool AclSave() => true;

    [ScriptFunctionDefinition("aclSetRight")]
    public bool AclSetRight(AclEntry acl, string rightName, bool hasAccess) => true;

    [ScriptFunctionDefinition("hasObjectPermissionTo")]
    public bool HasObjectPermissionTo(LuaValue theObject, string action, bool defaultPermission = true) => true;

    [ScriptFunctionDefinition("isObjectInACLGroup")]
    public bool IsObjectInACLGroup(string objectName, AclGroup group) => true;
}
