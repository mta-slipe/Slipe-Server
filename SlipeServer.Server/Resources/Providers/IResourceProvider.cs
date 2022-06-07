using System.Collections.Generic;

namespace SlipeServer.Server.Resources.Providers;

public interface IResourceProvider
{
    public ushort ReserveNetId();
    public Resource GetResource(string name);
    public IEnumerable<Resource> GetResources();
    public void Refresh();
}
