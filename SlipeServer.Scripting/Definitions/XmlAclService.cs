using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SlipeServer.Scripting.Definitions;

public class XmlAclService : IAclService
{
    private readonly string filePath;
    private Dictionary<string, AclEntry> acls = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, AclGroup> groups = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<(string AclName, string RightName), (string? Who, bool? Pending, string? Date)> rightMetadata = new();

    public XmlAclService() : this("acl.xml") { }

    public XmlAclService(string filePath)
    {
        this.filePath = filePath;
        Load();
    }

    #region Structural operations

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

    public bool DestroyAcl(AclEntry acl)
    {
        foreach (var group in this.groups.Values)
            group.Acls.Remove(acl);
        return this.acls.Remove(acl.Name);
    }

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

    #endregion

    #region Rights

    public bool SetRight(AclEntry acl, string rightName, bool hasAccess)
    {
        acl.Rights[rightName] = hasAccess;
        return true;
    }

    public bool GetRight(AclEntry acl, string rightName) =>
        acl.Rights.TryGetValue(rightName, out var access) ? access : false;

    public bool RemoveRight(AclEntry acl, string rightName)
    {
        acl.Rights.Remove(rightName);
        this.rightMetadata.Remove((acl.Name, rightName));
        return true;
    }

    public IEnumerable<string> ListRights(AclEntry acl, string? allowedType = null)
    {
        var rights = acl.Rights.Keys.AsEnumerable();
        if (!string.IsNullOrEmpty(allowedType) && allowedType != "all")
            rights = rights.Where(r => r.StartsWith(allowedType + ".", StringComparison.OrdinalIgnoreCase));
        return rights;
    }

    #endregion

    #region Group membership

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

    public IEnumerable<AclGroup> GetGroupsForObject(string objectName) =>
        this.groups.Values.Where(g => IsObjectInAclGroup(objectName, g));

    #endregion

    #region Permission checks

    public bool HasObjectPermissionTo(string objectName, string action, bool defaultPermission = true)
    {
        bool? result = null;
        foreach (var group in this.groups.Values)
        {
            if (!IsObjectInAclGroup(objectName, group))
                continue;
            foreach (var acl in group.Acls)
            {
                if (!acl.Rights.TryGetValue(action, out var access))
                    continue;
                if (!access)
                    return false;
                result = true;
            }
        }
        return result ?? defaultPermission;
    }

    public bool IsObjectInAclGroup(string objectName, AclGroup group) =>
        group.Objects.Any(pattern => MatchesObjectPattern(objectName, pattern));

    #endregion

    #region Persistence

    public bool Reload()
    {
        this.acls = new Dictionary<string, AclEntry>(StringComparer.OrdinalIgnoreCase);
        this.groups = new Dictionary<string, AclGroup>(StringComparer.OrdinalIgnoreCase);
        this.rightMetadata.Clear();
        Load();
        return true;
    }

    public bool Save()
    {
        var root = new XElement("acl");

        foreach (var group in this.groups.Values)
        {
            var groupElement = new XElement("group", new XAttribute("name", group.Name));
            foreach (var acl in group.Acls)
                groupElement.Add(new XElement("acl", new XAttribute("name", acl.Name)));
            foreach (var obj in group.Objects)
                groupElement.Add(new XElement("object", new XAttribute("name", obj)));
            root.Add(groupElement);
        }

        foreach (var acl in this.acls.Values)
        {
            var aclElement = new XElement("acl", new XAttribute("name", acl.Name));
            foreach (var (rightName, access) in acl.Rights)
            {
                var rightElement = new XElement("right",
                    new XAttribute("name", rightName),
                    new XAttribute("access", access ? "true" : "false"));

                if (this.rightMetadata.TryGetValue((acl.Name, rightName), out var meta))
                {
                    if (meta.Who != null) rightElement.Add(new XAttribute("who", meta.Who));
                    if (meta.Pending.HasValue) rightElement.Add(new XAttribute("pending", meta.Pending.Value ? "true" : "false"));
                    if (meta.Date != null) rightElement.Add(new XAttribute("date", meta.Date));
                }

                aclElement.Add(rightElement);
            }
            root.Add(aclElement);
        }

        new XDocument(root).Save(filePath);
        return true;
    }

    #endregion

    #region Internal helpers

    private void Load()
    {
        if (!File.Exists(filePath))
            return;

        var doc = XDocument.Load(filePath);
        var root = doc.Root!;

        foreach (var aclElement in root.Elements("acl"))
        {
            var name = (string)aclElement.Attribute("name")!;
            var entry = new AclEntry(name);

            foreach (var rightElement in aclElement.Elements("right"))
            {
                var rightName = (string)rightElement.Attribute("name")!;
                var access = (string?)rightElement.Attribute("access") == "true";
                entry.Rights[rightName] = access;

                var who = (string?)rightElement.Attribute("who");
                var pendingStr = (string?)rightElement.Attribute("pending");
                var date = (string?)rightElement.Attribute("date");
                bool? pending = pendingStr != null ? pendingStr == "true" : null;

                if (who != null || pending.HasValue || date != null)
                    this.rightMetadata[(name, rightName)] = (who, pending, date);
            }

            this.acls[name] = entry;
        }

        foreach (var groupElement in root.Elements("group"))
        {
            var name = (string)groupElement.Attribute("name")!;
            var group = new AclGroup(name);

            foreach (var aclRef in groupElement.Elements("acl"))
            {
                var aclName = (string)aclRef.Attribute("name")!;
                if (this.acls.TryGetValue(aclName, out var aclEntry))
                    group.Acls.Add(aclEntry);
            }

            foreach (var objectElement in groupElement.Elements("object"))
                group.Objects.Add((string)objectElement.Attribute("name")!);

            this.groups[name] = group;
        }
    }

    private static bool MatchesObjectPattern(string objectName, string pattern)
    {
        if (pattern.EndsWith(".*", StringComparison.Ordinal))
        {
            var prefix = pattern[..^1];
            return objectName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
        return string.Equals(objectName, pattern, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
