using SlipeServer.Packets.Structs;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources.Interpreters.Meta;
using SlipeServer.Server.Resources.Providers;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using static SlipeServer.DropInReplacement.MixedResources.MixedResource;

namespace SlipeServer.DropInReplacement.MixedResources;

public class DropInReplacementResourceInterpreter : IResourceInterpreter
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
        var files = resourceProvider.GetFilesForResource(name);
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

        var resourceFiles = new List<ResourceFile>();

        var metaFile = files["meta.xml"];
        var xmlContent = Encoding.UTF8.GetString(metaFile);

        if (xmlContent.Contains("edf:") && !xmlContent.Contains("xmlns:edf"))
            xmlContent = Regex.Replace(xmlContent, @"<meta(?=[\s>])", "<meta xmlns:edf=\"urn:edf\"", RegexOptions.IgnoreCase);

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));
        using var xmlReader = XmlReader.Create(ms);;
        var serializer = new XmlSerializer(typeof(MetaXml));
        var meta = (MetaXml?)serializer.Deserialize(xmlReader);

        if (meta == null)
            throw new Exception($"Unable to parse meta file for resource {name}");

        var resource = new MixedResource(mtaServer, rootElement, name, path)
        {
            PriorityGroup = (meta.Value.downloadPriorityGroup != null && meta.Value.downloadPriorityGroup.Length > 0) ? meta.Value.downloadPriorityGroup.First().Data : 0,
            Files = GetFilesForMetaXmlResource(meta.Value, files),
            ServerFiles = GetServerFilesForMetaXmlResource(meta.Value, files),
            Exports = [.. GetExportsForMetaXmlResource(meta.Value)],
            ServerExports = [.. GetServerExportsForMetaXmlResource(meta.Value)],
            NoClientScripts = GetNoCacheFiles(meta.Value, files),
            IsOopEnabled = meta.Value.oops != null && meta.Value.oops.Any(x => x.Data.ToLower() == "true"),
            Settings = GetSettingsForMetaXmlResource(meta.Value),
            Info = GetInfoForMetaXmlResource(meta.Value)
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
        var resourceFiles = new List<ResourceFile>();

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
            foreach (var file in meta.scripts.Where(x => (x.Type == "client" || x.Type == "shared") && x.Cache != "false"))
            {
                foreach (var source in ExpandGlob(file.Source, files.Keys))
                    resourceFiles.Add(ResourceFileFactory.FromBytes(files[source], source, ResourceFileType.ClientScript));
            }
        }

        if (meta.configs != null)
        {
            foreach (var file in meta.configs.Where(x => (x.Type == "client" || x.Type == "shared")))
            {
                foreach (var source in ExpandGlob(file.Source, files.Keys))
                    resourceFiles.Add(ResourceFileFactory.FromBytes(files[source], source, ResourceFileType.ClientConfig));
            }
        }

        return resourceFiles;
    }

    private List<ServerResourceFile> GetServerFilesForMetaXmlResource(MetaXml meta, Dictionary<string, byte[]> files)
    {
        var resourceFiles = new List<ServerResourceFile>();

        if (meta.scripts != null)
        {
            foreach (var file in meta.scripts.Where(x => (x.Type == null || x.Type == "" || x.Type == "server" || x.Type == "shared")))
            {
                foreach (var source in ExpandGlob(file.Source, files.Keys))
                    resourceFiles.Add(new ServerResourceFile()
                    {
                        Content = files[source],
                        FileType = ResourceFileType.Script,
                        Name = source
                    });
            }
        }

        return resourceFiles;
    }

    private Dictionary<string, byte[]> GetNoCacheFiles(MetaXml meta, Dictionary<string, byte[]> files)
    {
        if (meta.scripts == null)
            return [];

        var result = new Dictionary<string, byte[]>();
        foreach (var file in meta.scripts.Where(x => (x.Type == "client" || x.Type == "shared") && x.Cache == "false"))
        {
            foreach (var source in ExpandGlob(file.Source, files.Keys))
                result[source] = files[source];
        }
        return result;
    }

    private IEnumerable<string> GetExportsForMetaXmlResource(MetaXml meta)
    {
        if (meta.exports == null)
            return [];

        return meta.exports
            .Where(x => x.Type == "client" || x.Type == "shared")
            .Select(x => x.Function);
    }

    private Dictionary<string, string> GetSettingsForMetaXmlResource(MetaXml meta)
    {
        if (meta.settings == null)
            return [];

        return meta.settings
            .ToDictionary(s => StripSettingPrefix(s.Name), s => s.Value);
    }

    private static string StripSettingPrefix(string name)
    {
        if (name.StartsWith('*') || name.StartsWith('@'))
            return name.Substring(1);
        return name;
    }

    private IEnumerable<string> GetServerExportsForMetaXmlResource(MetaXml meta)
    {
        if (meta.exports == null)
            return [];

        return meta.exports
            .Where(x => x.Type == "server" || x.Type == "shared")
            .Select(x => x.Function);
    }

    private static Dictionary<string, string> GetInfoForMetaXmlResource(MetaXml meta)
    {
        if (meta.info?.Attributes == null)
            return [];

        return meta.info.Attributes
            .ToDictionary(a => a.LocalName, a => a.Value);
    }
}
