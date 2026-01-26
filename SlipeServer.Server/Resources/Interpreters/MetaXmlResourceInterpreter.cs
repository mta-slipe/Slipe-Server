using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Resources.Interpreters.Meta;
using SlipeServer.Server.Resources.Providers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SlipeServer.Server.Resources.Interpreters;

public class MetaXmlResourceInterpreter : IResourceInterpreter
{
    public bool IsFallback => false;

    public bool TryInterpretResource(
        IMtaServer mtaServer,
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

        resource = GetResource(mtaServer, rootElement, resourceProvider, name, path, files);

        return true;
    }

    private Resource GetResource(MtaServer mtaServer, RootElement rootElement, IResourceProvider resourceProvider, string name, string path, IEnumerable<string> fileNames)
    {
        var files = fileNames.ToDictionary(x => x.Replace(Path.DirectorySeparatorChar, '/'), file => resourceProvider.GetFileContent(name, file));

        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        var metaFile = files["meta.xml"];
        var reader = new StringReader(Encoding.Default.GetString(metaFile));
        var serializer = new XmlSerializer(typeof(MetaXml));
        var meta = (MetaXml?)serializer.Deserialize(reader);

        if (meta == null)
            throw new System.Exception($"Unable to parse meta file for resource {name}");

        var resource = new Resource(mtaServer, rootElement, name, path)
        {
            PriorityGroup = (meta.Value.downloadPriorityGroup != null && meta.Value.downloadPriorityGroup.Length > 0) ? meta.Value.downloadPriorityGroup.First().Data : 0,
            Files = GetFilesForMetaXmlResource(meta.Value, files),
            Exports = GetExportsForMetaXmlResource(meta.Value).ToList(),
            NoClientScripts = GetNoCacheFiles(meta.Value, files),
            IsOopEnabled = meta.Value.oops != null && meta.Value.oops.Any(x => x.Data.ToLower() == "true")
        };
        return resource;
    }

    private List<ResourceFile> GetFilesForMetaXmlResource(MetaXml meta, Dictionary<string, byte[]> files)
    {
        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        if (meta.files != null)
        {
            foreach (var file in meta.files)
            {
                resourceFiles.Add(ResourceFileFactory.FromBytes(files[file.Source], file.Source, ResourceFileType.ClientFile));
            }
        }

        if (meta.scripts != null)
        {
            foreach (var file in meta.scripts.Where(x => x.Type == "client" && x.Cache != "false"))
            {
                resourceFiles.Add(ResourceFileFactory.FromBytes(files[file.Source], file.Source, ResourceFileType.ClientScript));
            }
        }

        if (meta.configs != null)
        {
            foreach (var file in meta.configs.Where(x => x.Type == "client"))
            {
                resourceFiles.Add(ResourceFileFactory.FromBytes(files[file.Source], file.Source, ResourceFileType.ClientConfig));
            }
        }

        return resourceFiles;
    }

    private Dictionary<string, byte[]> GetNoCacheFiles(MetaXml meta, Dictionary<string, byte[]> files)
    {
        return meta.scripts
            .Where(x => x.Type == "client" && x.Cache == "false")
            .ToDictionary(x => x.Source, x => files[x.Source]);
    }

    private IEnumerable<string> GetExportsForMetaXmlResource(MetaXml meta)
    {
        if(meta.exports == null)
            return Enumerable.Empty<string>();

        return meta.exports
            .Where(x => x.Type == "client" || x.Type == "shared")
            .Select(x => x.Function);
    }
}
