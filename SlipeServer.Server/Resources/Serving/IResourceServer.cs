using System.Collections.Generic;

namespace SlipeServer.Server.Resources.Serving;

public interface IResourceServer
{
    void Start();
    void Stop();
    void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files);
    void RemoveAdditionalResource(Resource resource);
}
