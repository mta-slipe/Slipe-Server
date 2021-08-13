using SlipeServer.Server.Loaders.Map.ElementsDefinitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    }

    public class DefaultMapLoader : MapLoader<DefaultMap>
    {
        public DefaultMapLoader() : base()
        {
        }

    }
}
