using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Resources;
using System.Collections.Generic;

namespace SlipeServer.Server.TestTools;

public class TestResourceProvider : IResourceProvider
{
    private readonly Dictionary<string, Resource> resources = [];
    private readonly object netIdLock = new();
    private ushort netId = 0;

    public void AddResource(Resource resource)
    {
        this.resources[resource.Name] = resource;
    }

    public Resource GetResource(string name)
    {
        return this.resources[name];
    }

    public IEnumerable<Resource> GetResources()
    {
        return this.resources.Values;
    }

    public void Refresh() { }

    private IEnumerable<Resource> IndexResourceDirectory(string directory) => [];

    public IEnumerable<string> GetFilesForResource(string name) => [];

    public byte[] GetFileContent(string resource, string file) => [];

    public ushort ReserveNetId()
    {
        lock (this.netIdLock)
            return this.netId++;
    }

    public void AddResourceInterpreter(IResourceInterpreter resourceInterpreter) { }
}
