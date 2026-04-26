using System.Xml;
using System.Xml.Serialization;

namespace SlipeServer.Server.Resources.Interpreters.Meta;

[XmlRoot("meta")]
public struct MetaXml()
{
    [XmlElement("info")]
    public MetaXmlInfo? info = null;

    [XmlElement("file")]
    public MetaXmlFile[] files = [];

    [XmlElement("config")]
    public MetaXmlConfig[] configs = [];

    [XmlElement("script")]
    public MetaXmlScript[] scripts = [];

    [XmlElement("export")]
    public MetaXmlExport[] exports = [];

    [XmlElement("map")]
    public MetaXmlMap[] maps = [];

    [XmlElement("include")]
    public MetaXmlInclude[] includes = [];

    [XmlElement("html")]
    public MetaXmlHtml[] htmls = [];

    [XmlElement("min_mta_version")]
    public MetaXmlMinMtaVersion[] minMtaVersions = [];

    [XmlElement("aclrequest")]
    public MetaXmlAclRequest[] aclRequests = [];

    [XmlElement("sync_map_element_data")]
    public MetaXmlSyncMapElementData[] syncMapElementData = [];

    [XmlElement("oop")]
    public MetaXmlOop[] oops = [];

    [XmlElement("download_priority_group")]
    public MetaXmlDownloadPriorityGroup[] downloadPriorityGroup = [];

    [XmlArray("settings")]
    [XmlArrayItem("setting")]
    public MetaXmlSetting[] settings = [];
}

public struct MetaXmlFile
{
    [XmlAttribute("src")]
    public string Source { get; set; }

    [XmlAttribute("download")]
    public string Download { get; set; }
}

public struct MetaXmlConfig
{
    [XmlAttribute("src")]
    public string Source { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }
}

public struct MetaXmlMap
{
    [XmlAttribute("src")]
    public string Source { get; set; }

    [XmlAttribute("dimension")]
    public string Dimension { get; set; }
}

public struct MetaXmlInclude
{
    [XmlAttribute("resource")]
    public string Resource { get; set; }

    [XmlAttribute("minversion")]
    public string MinVersion { get; set; }

    [XmlAttribute("maxversion")]
    public string MaxVersion { get; set; }
}

public struct MetaXmlHtml
{
    [XmlAttribute("src")]
    public string Source { get; set; }

    [XmlAttribute("default")]
    public string Default { get; set; }

    [XmlAttribute("raw")]
    public string Raw { get; set; }
}

public struct MetaXmlMinMtaVersion
{
    [XmlAttribute("client")]
    public string Client { get; set; }

    [XmlAttribute("server")]
    public string Server { get; set; }

    [XmlAttribute("both")]
    public string Both { get; set; }
}

public struct MetaXmlAclRequest
{
    [XmlElement("right")]
    public MetaXmlAclRight[] Rights { get; set; }
}

public struct MetaXmlAclRight
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("access")]
    public string Access { get; set; }
}

public struct MetaXmlSyncMapElementData
{
    [XmlText]
    public string Data { get; set; }
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

    [XmlAttribute("validate")]
    public string Validate { get; set; }
}

public struct MetaXmlExport
{
    [XmlAttribute("function")]
    public string Function { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlAttribute("http")]
    public string Http { get; set; }
}

public struct MetaXmlSetting
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("value")]
    public string Value { get; set; }
}

public class MetaXmlInfo
{
    [XmlAnyAttribute]
    public XmlAttribute[]? Attributes { get; set; }
}
