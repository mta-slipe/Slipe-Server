using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SlipeServer.Server.Loaders.Map.ElementsDefinitions
{
    public class WorldObjectDefinition : IdDefinition
    {
        [XmlAttribute("model")]
        public int Model { get; set; }
        [XmlAttribute("breakable")]
        public bool Breakable { get; set; }
        [XmlAttribute("interior")]
        public int Interior { get; set; }
        [XmlAttribute("alpha")]
        public int Alpha { get; set; }
        [XmlAttribute("doublesided")]
        public bool Doublesided { get; set; }
        [XmlAttribute("scale")]
        public int Scale { get; set; }
        [XmlAttribute("dimension")]
        public int Dimension { get; set; }
        [XmlAttribute("posX")]
        public float PosX { get; set; }
        [XmlAttribute("posY")]
        public float PosY { get; set; }
        [XmlAttribute("posZ")]
        public float PosZ { get; set; }
        [XmlAttribute("rotX")]
        public float RotX { get; set; }
        [XmlAttribute("rotY")]
        public float RotY { get; set; }
        [XmlAttribute("rotZ")]
        public float RotZ { get; set; }
    }
}
