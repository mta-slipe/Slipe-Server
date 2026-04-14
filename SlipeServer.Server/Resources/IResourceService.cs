using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Resources;

public interface IResourceService
{
    IReadOnlyCollection<Resource> StartedResources { get; }

    Resource? StartResource(string name);
    void StopResource(string name);
    void StopResource(Resource resource);

    event Action<Resource>? ResourceStarting;
    event Action<Resource>? ResourceStarted;
    event Action<Resource>? ResourceStopped;
}
