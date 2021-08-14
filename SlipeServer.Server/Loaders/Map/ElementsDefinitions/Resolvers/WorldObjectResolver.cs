using SlipeServer.Server.Elements;
using SlipeServer.Server.Loaders.Map.ElementsDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loaders.Map.Resolvers
{
    public class WorldObjectResolver : IGenericElementSupport<WorldObject, WorldObjectDefinition>
    {
        public WorldObjectResolver()
        {

        }

        public WorldObject Create(WorldObjectDefinition definition)
        {
            throw new NotImplementedException();
        }
    }
}
