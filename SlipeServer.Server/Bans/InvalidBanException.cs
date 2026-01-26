using System;

namespace SlipeServer.Server.Bans;

public class InvalidBanException(string? message) : Exception(message)
{
}
