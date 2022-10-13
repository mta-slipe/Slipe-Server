using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.IO;

namespace SlipeServer.Server.Resources.Providers;

public class FileSystemResourceProvider : IResourceProvider
{
    private readonly MtaServer mtaServer;
    private readonly RootElement rootElement;
    private readonly Configuration configuration;
    private readonly Dictionary<string, Resource> resources;
    private ushort netId = 0;

    public FileSystemResourceProvider(MtaServer mtaServer, RootElement rootElement, Configuration configuration)
    {
        this.mtaServer = mtaServer;
        this.rootElement = rootElement;
        this.configuration = configuration;
        this.resources = new();
        this.Refresh();
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
                var resource = new Resource(this.mtaServer, this.rootElement, name, subDirectory)
                {
                    NetId = this.ReserveNetId(),
                    Files = GetFilesForResource(subDirectory)
                };
                resources.Add(resource);
            }
        }

        return resources;
    }

    public List<ResourceFile> GetFilesForResource(string path)
    {
        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        foreach (var file in Directory.GetFiles(path))
        {
            byte[] content = File.ReadAllBytes(file);
            string fileName = Path.GetRelativePath(path, file);
            resourceFiles.Add(ResourceFileFactory.FromBytes(content, fileName));
        }

        return resourceFiles;
    }

    public ushort ReserveNetId() => this.netId++;
}
