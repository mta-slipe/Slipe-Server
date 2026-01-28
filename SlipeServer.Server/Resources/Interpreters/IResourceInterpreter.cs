using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Providers;

namespace SlipeServer.Server.Resources.Interpreters;

public interface IResourceInterpreter
{
    bool IsFallback { get; }

    bool TryInterpretResource(
        IMtaServer mtaServer,
        IRootElement rootElement,
        string name,
        string path,
        IResourceProvider resourceProvider,
        out Resource? resource
    );
}
