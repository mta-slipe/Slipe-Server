using System;

namespace SlipeServer.Server.Bans;

public class BanEventArgs(Ban ban) : EventArgs
{
    public Ban Ban { get; } = ban;
}
