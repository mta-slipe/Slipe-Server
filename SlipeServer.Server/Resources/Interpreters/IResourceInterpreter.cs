using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Providers;

namespace SlipeServer.Server.Resources.Interpreters;

public interface IResourceInterpreter
{
    public bool IsFallback { get; }

    public bool TryInterpretResource(
        IMtaServer mtaServer,
        RootElement rootElement,
        string name,
        string path,
        IResourceProvider resourceProvider,
        out Resource? resource
    );
}
