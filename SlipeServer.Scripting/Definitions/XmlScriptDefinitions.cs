using SlipeServer.Packets.Definitions.Lua;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SlipeServer.Scripting.Definitions;

public class XmlScriptDefinitions
{
    private readonly string basePath;

    public XmlScriptDefinitions()
    {
        this.basePath = Directory.GetCurrentDirectory();
    }

    private string ResolvePath(string filePath)
    {
        if (Path.IsPathRooted(filePath))
            return filePath;

        return Path.GetFullPath(Path.Combine(this.basePath, filePath));
    }

    [ScriptFunctionDefinition("xmlCreateFile")]
    public ScriptXmlNode XmlCreateFile(string filePath, string rootNodeName)
    {
        var fullPath = ResolvePath(filePath);
        var dir = Path.GetDirectoryName(fullPath);
        if (dir != null)
            Directory.CreateDirectory(dir);

        var element = new XElement(rootNodeName);
        return new ScriptXmlNode(element, fullPath);
    }

    [ScriptFunctionDefinition("xmlLoadFile")]
    public ScriptXmlNode? XmlLoadFile(string filePath, bool readOnly = false)
    {
        var fullPath = ResolvePath(filePath);
        if (!File.Exists(fullPath))
            return null;

        var doc = XDocument.Load(fullPath);
        if (doc.Root == null)
            return null;

        return new ScriptXmlNode(doc.Root, fullPath, readOnly);
    }

    [ScriptFunctionDefinition("xmlLoadString")]
    public ScriptXmlNode? XmlLoadString(string xmlString)
    {
        var element = XElement.Parse(xmlString);
        return new ScriptXmlNode(element);
    }

    [ScriptFunctionDefinition("xmlSaveFile")]
    public bool XmlSaveFile(ScriptXmlNode rootNode)
    {
        if (rootNode.FilePath == null || rootNode.ReadOnly)
            return false;

        var dir = Path.GetDirectoryName(rootNode.FilePath);
        if (dir != null)
            Directory.CreateDirectory(dir);

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), rootNode.Element);
        doc.Save(rootNode.FilePath);
        return true;
    }

    [ScriptFunctionDefinition("xmlUnloadFile")]
    public bool XmlUnloadFile(ScriptXmlNode node)
    {
        return true;
    }

    [ScriptFunctionDefinition("xmlCopyFile")]
    public ScriptXmlNode? XmlCopyFile(ScriptXmlNode nodeToCopy, string newFilePath)
    {
        var fullPath = ResolvePath(newFilePath);
        var copy = new XElement(nodeToCopy.Element);
        return new ScriptXmlNode(copy, fullPath);
    }

    [ScriptFunctionDefinition("xmlCreateChild")]
    public ScriptXmlNode XmlCreateChild(ScriptXmlNode parentNode, string tagName)
    {
        var child = new XElement(tagName);
        parentNode.Element.Add(child);
        return new ScriptXmlNode(child);
    }

    [ScriptFunctionDefinition("xmlDestroyNode")]
    public bool XmlDestroyNode(ScriptXmlNode node)
    {
        node.Element.Remove();
        return true;
    }

    [ScriptFunctionDefinition("xmlFindChild")]
    public ScriptXmlNode? XmlFindChild(ScriptXmlNode parent, string tagName, int index)
    {
        var children = parent.Element.Elements(tagName).ToList();
        if (index >= 0 && index < children.Count)
            return new ScriptXmlNode(children[index]);

        return null;
    }

    [ScriptFunctionDefinition("xmlNodeGetAttribute")]
    public string? XmlNodeGetAttribute(ScriptXmlNode node, string name)
    {
        return node.Element.Attribute(name)?.Value;
    }

    [ScriptFunctionDefinition("xmlNodeGetAttributes")]
    public LuaValue XmlNodeGetAttributes(ScriptXmlNode node)
    {
        var table = node.Element.Attributes()
            .ToDictionary(
                a => new LuaValue(a.Name.LocalName),
                a => new LuaValue(a.Value)
            );
        return new LuaValue(table);
    }

    [ScriptFunctionDefinition("xmlNodeGetChildren")]
    public object? XmlNodeGetChildren(ScriptXmlNode parent, int index = -1)
    {
        var children = parent.Element.Elements().ToList();
        if (index >= 0)
        {
            if (index < children.Count)
                return new ScriptXmlNode(children[index]);
            return null;
        }
        return children.Select(c => new ScriptXmlNode(c)).ToList();
    }

    [ScriptFunctionDefinition("xmlNodeGetName")]
    public string XmlNodeGetName(ScriptXmlNode node)
    {
        return node.Element.Name.LocalName;
    }

    [ScriptFunctionDefinition("xmlNodeGetParent")]
    public ScriptXmlNode? XmlNodeGetParent(ScriptXmlNode node)
    {
        var parent = node.Element.Parent;
        if (parent == null)
            return null;

        return new ScriptXmlNode(parent);
    }

    [ScriptFunctionDefinition("xmlNodeGetValue")]
    public string XmlNodeGetValue(ScriptXmlNode node)
    {
        return node.Element.Value;
    }

    [ScriptFunctionDefinition("xmlNodeSetAttribute")]
    public bool XmlNodeSetAttribute(ScriptXmlNode node, string name, LuaValue value)
    {
        var stringValue = value.StringValue
            ?? value.FloatValue?.ToString(CultureInfo.InvariantCulture)
            ?? value.DoubleValue?.ToString(CultureInfo.InvariantCulture)
            ?? value.IntegerValue?.ToString(CultureInfo.InvariantCulture);

        node.Element.SetAttributeValue(name, stringValue);
        return true;
    }

    [ScriptFunctionDefinition("xmlNodeSetName")]
    public bool XmlNodeSetName(ScriptXmlNode node, string name)
    {
        node.Element.Name = name;
        return true;
    }

    [ScriptFunctionDefinition("xmlNodeSetValue")]
    public bool XmlNodeSetValue(ScriptXmlNode node, string value, bool setCDATA = false)
    {
        if (setCDATA)
            node.Element.ReplaceNodes(new XCData(value));
        else
            node.Element.Value = value;

        return true;
    }
}
