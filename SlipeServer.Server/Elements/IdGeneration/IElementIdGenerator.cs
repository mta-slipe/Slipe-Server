using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.IdGeneration;

public interface IElementIdGenerator
{
    public uint GetId();
}
