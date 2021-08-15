using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SlipeServer.Server.Loaders.Map.ElementsDefinitions
{
    public class VehicleDefinition : IdDefinition
    {
        [XmlAttribute("model")]
        public int Model { get; set; }
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
        public VehicleModel ObjectModel => (VehicleModel)Enum.Parse(typeof(VehicleModel), this.Model.ToString());
        [XmlIgnore]
        public Vector3 Position => new Vector3(this.PosX, this.PosY, this.PosZ);
        [XmlIgnore]
        public Vector3 Rotation => new Vector3(this.RotX, this.RotY, this.RotZ);
    }
}
