using SlipeServer.Server;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SlipeServer.Scripting.Definitions;

public class XmlFile : XmlNode
{
    private readonly XDocument document;
    private readonly bool readOnly;

    public XmlFile(string content, bool readOnly)
    {
        this.document = XDocument.Parse(content);
        this.readOnly = readOnly;
        this.element = this.document.Root;
    }
}

public class XmlNode
{
    protected XElement? element;

    public XmlNode(XElement? element = null)
    {
        if(element != null)
            this.element = element;
    }

    public string GetName()
    {
        return this.element!.Name.ToString();
    }

    public string? GetAttribute(string name)
    {
        return this.element!.Attribute(name)?.Value;
    }
    public Dictionary<string, string> GetAttributes()
    {
        return this.element!.Attributes().ToDictionary(x => x.Name.ToString(), x => x.Value);
    }

    public XmlNode? FindChild(string name, int index)
    {
        var node = this.element!.Elements(name).ElementAtOrDefault(index);
        if (node == null)
            return null;

        return new XmlNode(node);
    }

    public XmlNode? GetParent()
    {
        var node = this.element!.Parent;
        if (node == null)
            return null;

        return new XmlNode(node);
    }

    public XmlNode[] GetChildren(int? index = null)
    {
        if (index != null)
        {
            var node = this.element!.Elements().ElementAtOrDefault(index.Value);
            if (node == null)
                return [];
            return [new XmlNode(node)];
        } else
        {
            return this.element!.Elements().Select(x => new XmlNode(x)).ToArray();
        }
    }

    public void SetName(string name)
    {
        this.element!.Name = name;
    }

    public void SetValue(string value)
    {
        this.element!.Value = value;
    }

    public void SetAttributeValue(string name, string value)
    {
        this.element!.SetAttributeValue(name, value);
    }
}

public class XmlScriptDefinitions
{
    private readonly MtaServer server;

    public XmlScriptDefinitions(MtaServer server)
    {
        this.server = server;
    }

    [ScriptFunctionDefinition("xmlLoadFile")]
    public XmlFile XmlLoadFile(string fileName, bool readOnly = false)
    {
        // TODO: Use some kind of Resource file provider
        var xmlFileContent = File.ReadAllText(fileName);
        return new XmlFile(xmlFileContent, readOnly);
    }

    [ScriptFunctionDefinition("xmlNodeGetName")]
    public string XmlNodeGetName(XmlNode xmlNode)
    {
        return xmlNode.GetName();
    }

    [ScriptFunctionDefinition("xmlNodeGetAttribute")]
    public string? XmlNodeGetAttribute(XmlNode xmlNode, string name)
    {
        return xmlNode.GetName();
    }
    

    [ScriptFunctionDefinition("xmlNodeGetAttributes")]
    public Dictionary<string, string> XmlNodeGetAttributes(XmlNode xmlNode)
    {
        var results = xmlNode.GetAttributes();
        return results;
    }

    [ScriptFunctionDefinition("xmlFindChild")]
    public XmlNode? XmlFindChild(XmlNode parent, string tagName, int index)
    {
        return parent.FindChild(tagName, index);
    }

    [ScriptFunctionDefinition("xmlNodeGetParent")]
    public XmlNode? XmlNodeGetParent(XmlNode node)
    {
        return node.GetParent();
    }

    [ScriptFunctionDefinition("xmlNodeGetChildren")]
    public XmlNode[] XmlNodeGetChildren(XmlNode node, int? index = null)
    {
        return node.GetChildren(index);
    }

    [ScriptFunctionDefinition("xmlNodeSetAttribute")]
    public bool XmlNodeSetAttribute(XmlNode node, string name, string value)
    {
        node.SetAttributeValue(name, value);
        return true;
    }

    [ScriptFunctionDefinition("xmlNodeSetName")]
    public bool XmlNodeSetName(XmlNode node, string name)
    {
        node.SetName(name);
        return true;
    }

    [ScriptFunctionDefinition("xmlNodeSetValue")]
    public bool XmlNodeSetValue(XmlNode node, string name)
    {
        node.SetValue(name);
        return true;
    }
}
