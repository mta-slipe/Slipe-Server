using SlipeServer.Net.Wrappers.Enums;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SlipeServer.Server.Resources.Interpreters.Meta;

[XmlRoot("meta")]
public struct MetaXml
{
    [XmlElement("file")]
    public MetaXmlFile[] files;

    [XmlElement("config")]
    public MetaXmlConfig[] configs;

    [XmlElement("script")]
    public MetaXmlScript[] scripts;

    [XmlElement("export")]
    public MetaXmlExport[] exports;

    [XmlElement("oop")]
    public MetaXmlOop[] oops;

    [XmlElement("download_priority_group")]
    public MetaXmlDownloadPriorityGroup[] downloadPriorityGroup;

    public static MetaXml? Create(byte[] data)
    {
        XmlDocument xmlConfig = new XmlDocument();
        using MemoryStream ms = new MemoryStream(data);

        xmlConfig.Load(ms);

        var scripts = new List<MetaXmlScript>();
        var meta = new MetaXml();
        foreach (XmlNode node in xmlConfig.FirstChild.ChildNodes)
        {
            switch (node.Name.ToLower())
            {
                case "script":
                    var script = new MetaXmlScript();
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        switch (attribute.Name.ToLower())
                        {
                            case "src":
                                script.Source = attribute.Value;
                                break;
                            case "type":
                                script.Type = attribute.Value;
                                break;
                        }
                    }
                    scripts.Add(script);
                    break;
            }
        }

        meta.scripts = scripts.ToArray();

        return meta;
    }
}

public struct MetaXmlFile
{
    [XmlAttribute("src")]
    public string Source { get; set; }
}

public struct MetaXmlConfig
{
    [XmlAttribute("src")]
    public string Source { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }
}

public struct MetaXmlOop
{
    [XmlText()]
    public string Data { get; set; }
}

public struct MetaXmlDownloadPriorityGroup
{
    [XmlText()]
    public int Data { get; set; }
}

public struct MetaXmlScript
{
    [XmlAttribute("src")]
    public string Source { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlAttribute("cache")]
    public string Cache { get; set; }
}

public struct MetaXmlExport
{
    [XmlAttribute("function")]
    public string Function { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }
}
