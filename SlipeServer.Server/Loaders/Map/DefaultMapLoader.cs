using SlipeServer.Server.Elements;
using SlipeServer.Server.Loaders.Map.ElementsDefinitions;
using SlipeServer.Server.Loaders.Map.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SlipeServer.Server.Loaders.Map
{
    public class DefaultMap
    {
        [XmlElement("object")]
        public WorldObjectDefinition[] Objects { get; set; }
        [XmlElement("vehicle")]
        public VehicleDefinition[] Vehicles { get; set; }
    }

    public class DefaultMapLoader : MapLoader<DefaultMap>
    {
        public DefaultMapLoader() : base()
        {
        }

        public override void Resolve(Resolver<DefaultMap> resolver)
        {
            resolver.ResolveArray(e => e.Objects, e =>
            {
                return new WorldObject(Enums.ObjectModel.A51gatecontrol, Vector3.Zero);
            });
            resolver.ResolveArray(e => e.Vehicles, e =>
            {
                return new Vehicle(404, Vector3.Zero);
            });
        }
    }
}
