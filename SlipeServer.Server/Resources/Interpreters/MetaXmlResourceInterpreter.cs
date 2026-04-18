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
        IRootElement rootElement,
        string name,
        string path,
        IResourceProvider resourceProvider,
        out Resource? resource
    )
    {
        var files = resourceProvider.GetFilesForResource(path);
        if (!files.Contains("meta.xml"))
        {
            resource = null;
            return false;
        }

        resource = GetResource(mtaServer, rootElement, resourceProvider, name, path, files);

        return true;
    }

    private Resource GetResource(IMtaServer mtaServer, IRootElement rootElement, IResourceProvider resourceProvider, string name, string path, IEnumerable<string> fileNames)
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

    private static IEnumerable<string> ExpandGlob(string pattern, IEnumerable<string> fileKeys)
    {
        if (!pattern.Contains('*') && !pattern.Contains('?'))
            return fileKeys.Contains(pattern) ? [pattern] : [];

        return fileKeys.Where(key => System.IO.Enumeration.FileSystemName.MatchesSimpleExpression(pattern, key, ignoreCase: true));
    }

    private List<ResourceFile> GetFilesForMetaXmlResource(MetaXml meta, Dictionary<string, byte[]> files)
    {
        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        if (meta.files != null)
        {
            foreach (var file in meta.files)
            {
                foreach (var source in ExpandGlob(file.Source, files.Keys))
                    resourceFiles.Add(ResourceFileFactory.FromBytes(files[source], source, ResourceFileType.ClientFile));
            }
        }

        if (meta.scripts != null)
        {
            foreach (var file in meta.scripts.Where(x => x.Type == "client" && x.Cache != "false"))
            {
                foreach (var source in ExpandGlob(file.Source, files.Keys))
                    resourceFiles.Add(ResourceFileFactory.FromBytes(files[source], source, ResourceFileType.ClientScript));
            }
        }

        if (meta.configs != null)
        {
            foreach (var file in meta.configs.Where(x => x.Type == "client"))
            {
                foreach (var source in ExpandGlob(file.Source, files.Keys))
                    resourceFiles.Add(ResourceFileFactory.FromBytes(files[source], source, ResourceFileType.ClientConfig));
            }
        }

        return resourceFiles;
    }

    private Dictionary<string, byte[]> GetNoCacheFiles(MetaXml meta, Dictionary<string, byte[]> files)
    {
        var result = new Dictionary<string, byte[]>();
        foreach (var file in meta.scripts.Where(x => x.Type == "client" && x.Cache == "false"))
        {
            foreach (var source in ExpandGlob(file.Source, files.Keys))
                result[source] = files[source];
        }
        return result;
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
