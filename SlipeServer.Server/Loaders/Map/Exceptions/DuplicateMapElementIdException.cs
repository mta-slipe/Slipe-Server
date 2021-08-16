using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loaders.Map.Exceptions
{
    public class DuplicateMapElementIdException : MapLoaderExceptionBase
    {
        public string Id { get; set; }
        public DuplicateMapElementIdException(string id, string definitionName) : base($"Duplicated id: '{id}' for '{definitionName}'")
        {
            this.Id = id;
        }
    }
}
