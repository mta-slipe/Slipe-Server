using System;

namespace SlipeServer.Server.Bans;

public class InvalidBanException : Exception
{
    public InvalidBanException(string? message) : base(message)
    {
    }
}
