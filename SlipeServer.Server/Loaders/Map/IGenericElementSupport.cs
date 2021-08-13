using SlipeServer.Server.Loaders.Map.ElementsDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loaders.Map
{
    public class DuplicateElementSupport : Exception
    {
        public DuplicateElementSupport(string name) : base($"Found duplicated support for node {name}")
        {

        }
    }

    public interface IGenericElementSupport<TElement, TDefinition> where TDefinition: IdDefinition
    {
        public TElement Create(TDefinition definition);
    }
}
