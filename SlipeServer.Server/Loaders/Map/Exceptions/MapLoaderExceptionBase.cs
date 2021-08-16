using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loaders.Map.Exceptions
{
    public abstract class MapLoaderExceptionBase : Exception
    {
        public MapLoaderExceptionBase(string message) : base("Failed to load map, reason: " + message)
        {

        }
    }
}
