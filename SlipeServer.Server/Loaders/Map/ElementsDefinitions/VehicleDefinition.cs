using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SlipeServer.Server.Loaders.Map.ElementsDefinitions
{
    public class VehicleDefinition : IdDefinition
    {
        [XmlAttribute("model")]
        public int Model { get; set; }
    }
}
