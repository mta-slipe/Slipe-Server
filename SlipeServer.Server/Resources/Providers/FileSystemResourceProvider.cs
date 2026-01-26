using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Interpreters;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SlipeServer.Server.Resources.Providers;

/// <summary>
/// Basic resource provider that provides resources based on the files within the resource directory (as configured in the server's configuration)
/// </summary>
public class FileSystemResourceProvider(IMtaServer mtaServer) : IResourceProvider
{
    private readonly RootElement rootElement = mtaServer.RootElement;
    private readonly Configuration configuration = mtaServer.Configuration;
    private readonly Dictionary<string, Resource> resources = new();
    private readonly List<IResourceInterpreter> resourceInterpreters = new();

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
                    if (resourceInterpreter.TryInterpretResource(mtaServer, this.rootElement, name, subDirectory, this, out resource))
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
