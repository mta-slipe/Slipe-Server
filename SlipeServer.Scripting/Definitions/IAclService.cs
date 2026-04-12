using System.Collections.Generic;

namespace SlipeServer.Scripting.Definitions;

public interface IAclService
{
    AclEntry CreateAcl(string name);
    AclGroup CreateGroup(string name);
    bool DestroyAcl(AclEntry acl);
    bool DestroyGroup(AclGroup group);
    AclEntry? GetAcl(string name);
    AclGroup? GetGroup(string name);
    IEnumerable<AclEntry> ListAcls();
    IEnumerable<AclGroup> ListGroups();

    bool SetRight(AclEntry acl, string rightName, bool hasAccess);
    bool GetRight(AclEntry acl, string rightName);
    bool RemoveRight(AclEntry acl, string rightName);
    IEnumerable<string> ListRights(AclEntry acl, string? allowedType = null);

    bool GroupAddAcl(AclGroup group, AclEntry acl);
    bool GroupRemoveAcl(AclGroup group, AclEntry acl);
    bool GroupAddObject(AclGroup group, string objectName);
    bool GroupRemoveObject(AclGroup group, string objectName);
    IEnumerable<AclEntry> GroupListAcls(AclGroup group);
    IEnumerable<string> GroupListObjects(AclGroup group);
    IEnumerable<AclGroup> GetGroupsForObject(string objectName);

    bool HasObjectPermissionTo(string objectName, string action, bool defaultPermission = true);
    bool IsObjectInAclGroup(string objectName, AclGroup group);

    bool Reload();
    bool Save();
}
