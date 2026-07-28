using SlipeServer.Server.Elements;

namespace SlipeServer.Server.Resources;

public interface IResource
{
    string Name { get; }
    Element Root { get; }
}
