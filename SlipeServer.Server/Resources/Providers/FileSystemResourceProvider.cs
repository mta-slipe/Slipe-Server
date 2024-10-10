using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Interpreters;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlipeServer.Server.Resources.Providers;

/// <summary>
/// Basic resource provider that provides resources based on the files within the resource directory (as configured in the server's configuration)
/// </summary>
public class FileSystemResourceProvider : IResourceProvider
{
    private readonly MtaServer mtaServer;
    private readonly RootElement rootElement;
    private readonly Configuration configuration;
    private readonly Dictionary<string, Resource> resources;
    private readonly Dictionary<string, ServerResourceFiles> serverResources;
    private readonly List<IResourceInterpreter> resourceInterpreters;

    private readonly object netIdLock = new();
    private ushort netId = 0;

    public FileSystemResourceProvider(MtaServer mtaServer)
    {
        this.mtaServer = mtaServer;
        this.rootElement = mtaServer.RootElement;
        this.configuration = mtaServer.Configuration;
        this.resources = new();
        this.serverResources = new();
        this.resourceInterpreters = new();
    }

    public Resource GetResource(string name)
    {
        return this.resources[name];
    }

    public ServerResourceFiles GetServerResourceFiles(string name)
    {
        return this.serverResources[name];
    }

    public IEnumerable<Resource> GetResources()
    {
        return this.resources.Values;
    }
    
    public IReadOnlyDictionary<string, ServerResourceFiles> GetServerResourcesFiles()
    {
        return this.serverResources;
    }

    public void Refresh()
    {
        this.resources.Clear();
        this.serverResources.Clear();
        var (resources, serverResources) = IndexResourceDirectory(this.configuration.ResourceDirectory);

        foreach (var resource in resources)
            this.resources[resource.Name] = resource;
        foreach (var serverResource in serverResources)
            this.serverResources[serverResource.Name] = serverResource;
    }

    private (IEnumerable<Resource>, IEnumerable<ServerResourceFiles>) IndexResourceDirectory(string directory)
    {
        List<Resource> resources = new();
        List<ServerResourceFiles> serverResources = new();

        if (!Directory.Exists(directory))
            return (resources, serverResources);

        var directories = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly);
        foreach (var subDirectory in directories)
        {
            if (subDirectory.StartsWith("[") && subDirectory.EndsWith("]"))
            {
                var (subResource, subServerResource) = IndexResourceDirectory(subDirectory);
                foreach (var resource in subResource)
                    resources.Add(resource);
                foreach (var serverResource in subServerResource)
                    serverResources.Add(serverResource);
            }

            var name = Path.GetFileName(subDirectory)!;
            if (this.resources.ContainsKey(name))
            {
                resources.Add(this.resources[name]);
            } else
            {
                Resource? resource = null;
                foreach (var resourceInterpreter in this.resourceInterpreters)
                {
                    if (resourceInterpreter.TryInterpretResource(this.mtaServer, this.rootElement, name, subDirectory, this, out resource, out var serverResource))
                    {
                        resource!.NetId = this.ReserveNetId();
                        resources.Add(resource);
                        serverResources.Add(serverResource);
                        break;
                    }
                }

                if (resource == null)
                {
                    throw new System.Exception($"Unable to interpret resource {name}");
                }
            }
        }

        return (resources, serverResources);
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
