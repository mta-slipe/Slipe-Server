using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources.Providers;

namespace SlipeServer.Scripting.Lua.Tests.Tools;

public class TestResourceProvider : IResourceProvider
{
    private readonly Dictionary<string, Resource> resources = new(StringComparer.OrdinalIgnoreCase);
    private ushort nextNetId;

    public void AddResource(Resource resource)
    {
        resource.NetId = ReserveNetId();
        this.resources[resource.Name] = resource;
    }

    public ushort ReserveNetId() => this.nextNetId++;

    public Resource GetResource(string name) => this.resources[name];

    public IEnumerable<Resource> GetResources() => this.resources.Values;

    public void Refresh()
    {
    }

    public IEnumerable<string> GetFilesForResource(string name) => [];

    public byte[] GetFileContent(string resource, string file) => [];

    public void AddResourceInterpreter(IResourceInterpreter resourceInterpreter)
    {
    }
}
