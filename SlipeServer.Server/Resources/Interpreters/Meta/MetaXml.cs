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
