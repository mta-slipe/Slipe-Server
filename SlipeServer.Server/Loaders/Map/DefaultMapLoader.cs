using SlipeServer.Server.Elements;
using SlipeServer.Server.Loaders.Map.ElementsDefinitions;
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
        public WorldObjectDefinition[]? Objects { get; set; }
        [XmlElement("vehicle")]
        public VehicleDefinition[]? Vehicles { get; set; }
    }

    public class DefaultMapLoader : MapLoader<DefaultMap>
    {
        public DefaultMapLoader() : base()
        {
        }

        public override void Resolve(Resolver<DefaultMap> resolver)
        {
            resolver.Resolve(e => e.Objects, @object =>
            {
                return new WorldObject(@object.ObjectModel, @object.Position);
            });
            resolver.Resolve(e => e.Vehicles, vehicle =>
            {
                return new Vehicle((ushort)vehicle.VehicleModel, vehicle.Position);
            });
        }
    }
}
