using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SlipeServer.Server.Loaders.Map.ElementsDefinitions
{
    public class Element3DDefinition : IdDefinition
    {
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

        [XmlIgnore]
        public Vector3 Position => new Vector3(this.PosX, this.PosY, this.PosZ);
        [XmlIgnore]
        public Vector3 Rotation => new Vector3(this.RotX, this.RotY, this.RotZ);

    }
}
