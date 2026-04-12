using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerConnectingEventArgs(string name, string ip, string serial, string version) : EventArgs
{
    public string Name { get; } = name;
    public string IP { get; } = ip;
    public string Serial { get; } = serial;
    public string Version { get; } = version;
}
