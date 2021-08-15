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
    public class WorldObjectDefinition : IdDefinition
    {
        [XmlAttribute("model")]
        public int Model { get; set; }
        [XmlAttribute("breakable")]
        public bool Breakable { get; set; }
        [XmlAttribute("interior")]
        public byte Interior { get; set; }
        [XmlAttribute("doublesided")]
        public bool Doublesided { get; set; }
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
        [XmlAttribute("scale")]
        public float Scale { get; set; }
        [XmlAttribute("scaleX")]
        public float ScaleX { get; set; }
        [XmlAttribute("scaleY")]
        public float ScaleY { get; set; }
        [XmlAttribute("scaleZ")]
        public float ScaleZ { get; set; }
        [XmlAttribute("collisions")]
        public bool Collisions { get; set; }
        [XmlAttribute("alpha")]
        public byte Alpha { get; set; }
        [XmlAttribute("frozen")]
        public bool Frozen { get; set; }

        [XmlIgnore]
        public ObjectModel ObjectModel => (ObjectModel)Enum.Parse(typeof(ObjectModel), this.Model.ToString());
        [XmlIgnore]
        public Vector3 Position => new Vector3(this.PosX, this.PosY, this.PosZ);
        [XmlIgnore]
        public Vector3 Rotation => new Vector3(this.RotX, this.RotY, this.RotZ);

    }
}
