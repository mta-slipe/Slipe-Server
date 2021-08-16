using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loaders.Map.Exceptions
{
    public class InvalidMapElementIdException : MapLoaderExceptionBase
    {
        public InvalidMapElementIdException(bool wasEmpty, string definitionName) : base(wasEmpty ? $"Missing id for '{definitionName}'" : $"Invalid id for '{definitionName}'")
        {
        }
    }
}
