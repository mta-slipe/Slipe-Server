using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources.Providers;
using System.Xml.Linq;

namespace SlipeServer.DropInReplacement.MixedResources;

public class DropInReplacementResourceProvider(IMtaServer mtaServer, IRootElement rootElement) : IResourceProvider
{
    private readonly Configuration configuration = mtaServer.Configuration;

    private readonly Dictionary<string, Resource> resources = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, byte[]> resourceFileContentCache = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, string> pathsPerResourceName = [];

    private readonly List<IResourceInterpreter> resourceInterpreters = [];

    private readonly Lock netIdLock = new();
    private ushort netId = 0;

    public Resource GetResource(string name)
    {
        return this.resources[name];
    }

    public IEnumerable<Resource> GetResources()
    {
        return this.resources.Values;
    }

    public void Refresh()
    {
        this.resources.Clear();
        var resources = IndexResourceDirectory(this.configuration.ResourceDirectory);

        foreach (var resource in resources)
            this.resources[resource.Name] = resource;
    }

    private IEnumerable<Resource> IndexResourceDirectory(string directory)
    {
        List<Resource> resources = new();

        if (!Directory.Exists(directory))
            return resources;

        var directories = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly);
        foreach (var subDirectory in directories)
        {
            var directoryName = Path.GetFileName(subDirectory);
            if (directoryName.StartsWith('[') && directoryName.EndsWith(']'))
            {
                foreach (var resource in IndexResourceDirectory(subDirectory))
                    resources.Add(resource);
            } else
            {
                var name = Path.GetFileName(subDirectory)!;
                if (this.resources.ContainsKey(name))
                {
                    resources.Add(this.resources[name]);
                } else
                {
                    this.pathsPerResourceName[name] = subDirectory;

                    Resource? resource = null;
                    foreach (var resourceInterpreter in this.resourceInterpreters)
                    {
                        if (resourceInterpreter.TryInterpretResource(mtaServer, rootElement, name, subDirectory, this, out resource))
                        {
                            if (resource is not MixedResource mixedResource)
                                continue;

                            resource!.NetId = this.ReserveNetId();
                            resources.Add(resource);

                            break;
                        }
                    }

                    if (resource == null)
                    {
                        throw new System.Exception($"Unable to interpret resource {name}");
                    }
                }

            }
        }

        return resources;
    }

    public IEnumerable<string> GetFilesForResource(string name)
    {
        var path = this.pathsPerResourceName[name];
        //var path = Path.Join(this.configuration.ResourceDirectory, name);
        var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);

        return files.Select(file => Path.GetRelativePath(path, file));
    }

    public byte[] GetFileContent(string resource, string file)
    {
        var path = this.pathsPerResourceName[resource];
        return File.ReadAllBytes(Path.Join(path, file));
    }

    public ushort ReserveNetId()
    {
        lock (this.netIdLock)
            return this.netId++;
    }

    public void AddResourceInterpreter(IResourceInterpreter resourceInterpreter)
    {
        this.resourceInterpreters.Add(resourceInterpreter);
        this.resourceInterpreters.Sort((a, b) => (a.IsFallback ? 1 : 0) - (b.IsFallback ? 1 : 0));
    }
}
