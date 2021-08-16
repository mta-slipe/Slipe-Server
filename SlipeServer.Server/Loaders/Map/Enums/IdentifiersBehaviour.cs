using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loaders.Map.Enums
{
    public enum IdentifiersBehaviour
    {
        // Ignore missing/duplicated ids, if something wrong with id it gets generated automatically.
        Ignore,
        // Throw an exception if id is not set, invalid or duplicated.
        Throw,
    }
}
