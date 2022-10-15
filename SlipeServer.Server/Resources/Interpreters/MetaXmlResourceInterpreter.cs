using Newtonsoft.Json;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Resources.Providers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SlipeServer.Server.Resources.Interpreters;

[XmlRoot("meta")]
public struct MetaXml
{
    [XmlElement("file")]
    public MetaXmlFile[] files;

    [XmlElement("script")]
    public MetaXmlScript[] scripts;
}

public struct MetaXmlFile
{
    [XmlAttribute("src")]
    public string Source { get; set; }
}

public struct MetaXmlScript
{
    [XmlAttribute("src")]
    public string Source { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }
}

public class MetaXmlResourceInterpreter : IResourceInterpreter
{
    public bool IsFallback => false;

    public bool TryInterpretResource(
        MtaServer mtaServer,
        RootElement rootElement,
        string name,
        string path,
        IResourceProvider resourceProvider,
        out Resource? resource
    )
    {
        var files = resourceProvider.GetFilesForResource(name);
        if (!files.Contains("meta.xml"))
        {
            resource = null;
            return false;
        }

        List<ResourceFile> resourceFiles = GetFilesForMetaXmlResource(resourceProvider, name, path, files);

        resource = new Resource(mtaServer, rootElement, name, path)
        {
            Files = resourceFiles
        };

        return true;
    }

    private List<ResourceFile> GetFilesForMetaXmlResource(IResourceProvider resourceProvider, string name, string path, IEnumerable<string> fileNames)
    {
        var files = fileNames.ToDictionary(x => x, file => resourceProvider.GetFileContent(name, file));

        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        var metaFile = files["meta.xml"];
        var reader = new StringReader(Encoding.Default.GetString(metaFile));
        var serializer = new XmlSerializer(typeof(MetaXml));
        var meta = (MetaXml?)serializer.Deserialize(reader);

        if (meta == null)
            throw new System.Exception($"Unable to parse meta file for resource {name}");

        foreach (var file in meta.Value.files)
        {
            resourceFiles.Add(ResourceFileFactory.FromBytes(files[file.Source], file.Source, ResourceFileType.ClientFile));
        }

        foreach (var file in meta.Value.scripts.Where(x => x.Type == "client"))
        {
            resourceFiles.Add(ResourceFileFactory.FromBytes(files[file.Source], file.Source, ResourceFileType.ClientScript));
        }

        return resourceFiles;
    }

    private ResourceFile GetResourceFileForPath(Dictionary<string, byte[]> files, string path)
    {
        return ResourceFileFactory.FromBytes(files[path], path);
    }
}
