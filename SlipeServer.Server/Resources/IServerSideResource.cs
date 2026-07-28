using System.Collections.Generic;

namespace SlipeServer.Server.Resources;

/// <summary>
/// Implemented by resources that have server-side exported functions,
/// allowing the Lua <c>call()</c> function to invoke them without
/// advertising them to clients.
/// </summary>
public interface IServerSideResource : IResource
{
    IReadOnlyList<string> ServerExports { get; }
}
