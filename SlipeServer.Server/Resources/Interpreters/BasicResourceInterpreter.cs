using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Providers;
using System.Collections.Generic;

namespace SlipeServer.Server.Resources.Interpreters;

public class BasicResourceInterpreter : IResourceInterpreter
{
    public bool IsFallback => true;

    public bool TryInterpretResource(
        IMtaServer mtaServer,
        RootElement rootElement,
        string name,
        string path,
        IResourceProvider resourceProvider,
        out Resource? resource
    )
    {
        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        foreach (var file in resourceProvider.GetFilesForResource(name))
        {
            byte[] content = resourceProvider.GetFileContent(name, file);
            resourceFiles.Add(ResourceFileFactory.FromBytes(content, file));
        }

        resource = new Resource(mtaServer, rootElement, name, path)
        {
            Files = resourceFiles
        };

        return true;
    }
}
