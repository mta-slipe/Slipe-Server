using SlipeServer.Server.Enums;
using SlipeServer.Server.Loaders.Map.ElementsDefinitions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SlipeServer.Server.Loaders.Map.ElementsDefinitions
{
    public class WorldObjectDefinition : Element3DDefinition, IDefinitionName
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
        public ObjectModel ObjectModel
        {
            get
            {
                if (Enum.IsDefined(typeof(ObjectModel), this.Model))
                    if (Enum.TryParse(this.Model.ToString(), out ObjectModel value))
                        return value;
                throw new ArgumentException();
            }
        }
        [XmlIgnore]
        public string DefinitionName => "object";
    }
}
