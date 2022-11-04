using System;

namespace SlipeServer.Server.Elements.IdGeneration;

public class ElementIdsExhaustedException : Exception
{
    public ElementIdsExhaustedException() : base("Element Ids exhausted")
    {
    }
}
