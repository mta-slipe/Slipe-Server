using SlipeServer.Server.Elements;
using SlipeServer.Server.Loaders.Map.ElementsDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loaders.Map.Resolvers
{
    public class WorldObjectSupport : IGenericElementSupport<WorldObject, WorldObjectDefinition>
    {
        public WorldObjectSupport()
        {

        }

        public WorldObject Create(WorldObjectDefinition definition)
        {
            throw new NotImplementedException();
        }
    }
}
