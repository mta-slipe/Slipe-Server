using SlipeServer.Server.Resources.Interpreters;
using System.Collections.Generic;

namespace SlipeServer.Server.Resources.Providers;

public interface IResourceProvider
{
    public ushort ReserveNetId();
    public Resource GetResource(string name);
    public IEnumerable<Resource> GetResources();
    public void Refresh();

    public IEnumerable<string> GetFilesForResource(string name);
    public byte[] GetFileContent(string resource, string file);

    public void AddResourceInterpreter(IResourceInterpreter resourceInterpreter);
    ServerResourceFiles GetServerResourceFiles(string name);
    IEnumerable<ServerResourceFiles> GetServerResourcesFiles();
}
