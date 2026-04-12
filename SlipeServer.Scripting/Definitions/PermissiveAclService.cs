using System.Collections.Generic;

namespace SlipeServer.Scripting.Definitions;

public class PermissiveAclService : IAclService
{
    private readonly Dictionary<string, AclEntry> acls = new(System.StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AclGroup> groups = new(System.StringComparer.OrdinalIgnoreCase);

    public AclEntry CreateAcl(string name)
    {
        if (!this.acls.TryGetValue(name, out var entry))
        {
            entry = new AclEntry(name);
            this.acls[name] = entry;
        }
        return entry;
    }

    public AclGroup CreateGroup(string name)
    {
        if (!this.groups.TryGetValue(name, out var group))
        {
            group = new AclGroup(name);
            this.groups[name] = group;
        }
        return group;
    }

    public bool DestroyAcl(AclEntry acl) => this.acls.Remove(acl.Name);

    public bool DestroyGroup(AclGroup group) => this.groups.Remove(group.Name);

    public AclEntry? GetAcl(string name)
    {
        this.acls.TryGetValue(name, out var entry);
        return entry;
    }

    public AclGroup? GetGroup(string name)
    {
        this.groups.TryGetValue(name, out var group);
        return group;
    }

    public IEnumerable<AclEntry> ListAcls() => this.acls.Values;

    public IEnumerable<AclGroup> ListGroups() => this.groups.Values;

    public bool SetRight(AclEntry acl, string rightName, bool hasAccess)
    {
        acl.Rights[rightName] = hasAccess;
        return true;
    }

    public bool GetRight(AclEntry acl, string rightName) =>
        acl.Rights.TryGetValue(rightName, out var access) ? access : true;

    public bool RemoveRight(AclEntry acl, string rightName)
    {
        acl.Rights.Remove(rightName);
        return true;
    }

    public IEnumerable<string> ListRights(AclEntry acl, string? allowedType = null) =>
        FilterRights(acl.Rights.Keys, allowedType);

    public bool GroupAddAcl(AclGroup group, AclEntry acl)
    {
        if (!group.Acls.Contains(acl))
            group.Acls.Add(acl);
        return true;
    }

    public bool GroupRemoveAcl(AclGroup group, AclEntry acl)
    {
        group.Acls.Remove(acl);
        return true;
    }

    public bool GroupAddObject(AclGroup group, string objectName)
    {
        group.Objects.Add(objectName);
        return true;
    }

    public bool GroupRemoveObject(AclGroup group, string objectName)
    {
        group.Objects.Remove(objectName);
        return true;
    }

    public IEnumerable<AclEntry> GroupListAcls(AclGroup group) => group.Acls;

    public IEnumerable<string> GroupListObjects(AclGroup group) => group.Objects;

    public IEnumerable<AclGroup> GetGroupsForObject(string objectName) => this.groups.Values;

    public bool HasObjectPermissionTo(string objectName, string action, bool defaultPermission = true) => true;

    public bool IsObjectInAclGroup(string objectName, AclGroup group) => true;

    public bool Reload() => true;

    public bool Save() => true;

    private static IEnumerable<string> FilterRights(IEnumerable<string> rights, string? type)
    {
        if (string.IsNullOrEmpty(type) || type == "all")
            return rights;
        return System.Linq.Enumerable.Where(rights, r => r.StartsWith(type + ".", System.StringComparison.OrdinalIgnoreCase));
    }
}
