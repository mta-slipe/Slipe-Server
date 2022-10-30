using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Interpreters;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlipeServer.Server.Resources.Providers;

public class FileSystemResourceProvider : IResourceProvider
{
    private readonly MtaServer mtaServer;
    private readonly RootElement rootElement;
    private readonly Configuration configuration;
    private readonly Dictionary<string, Resource> resources;
    private readonly List<IResourceInterpreter> resourceInterpreters;

    private readonly object netIdLock = new();
    private ushort netId = 0;

    public FileSystemResourceProvider(MtaServer mtaServer, RootElement rootElement, Configuration configuration)
    {
        this.mtaServer = mtaServer;
        this.rootElement = rootElement;
        this.configuration = configuration;
        this.resources = new();
        this.resourceInterpreters = new();
    }

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
            if (subDirectory.StartsWith("[") && subDirectory.EndsWith("]"))
                foreach (var resource in IndexResourceDirectory(subDirectory))
                    resources.Add(resource);

            var name = Path.GetFileName(subDirectory)!;
            if (this.resources.ContainsKey(name))
            {
                resources.Add(this.resources[name]);
            } else
            {
                Resource? resource = null;
                foreach (var resourceInterpreter in this.resourceInterpreters)
                {
                    if (resourceInterpreter.TryInterpretResource(this.mtaServer, this.rootElement, name, subDirectory, this, out resource))
                    {
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

        return resources;
    }

    public IEnumerable<string> GetFilesForResource(string name)
    {
        var path = Path.Join(this.configuration.ResourceDirectory, name);
        var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);

        return files.Select(file => Path.GetRelativePath(path, file));
    }

    public byte[] GetFileContent(string resource, string file)
    {
        return File.ReadAllBytes(Path.Join(this.configuration.ResourceDirectory, resource, file));
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
