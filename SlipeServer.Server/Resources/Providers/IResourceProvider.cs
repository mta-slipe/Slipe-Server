using SlipeServer.Packets.Structs;
using System.Collections.Generic;

namespace SlipeServer.Server.Resources.Providers;

public interface IResourceProvider
{
    public Resource GetResource(string name);
    public IEnumerable<Resource> GetResources();
    public void Refresh();

    public IEnumerable<ResourceFile> GetFilesForResource(string name);
    public IEnumerable<ResourceFile> GetFilesForResource(Resource resource);
}
