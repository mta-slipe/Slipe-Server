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
    public class VehicleDefinition : Element3DDefinition
    {
        [XmlAttribute("model")]
        public int Model { get; set; }
        [XmlAttribute("variant1")]
        public int Variant1 { get; set; }
        [XmlAttribute("variant2")]
        public int Variant2 { get; set; }
        [XmlAttribute("turretX")]
        public float TurretX { get; set; }
        [XmlAttribute("turretY")]
        public float TurretY { get; set; }
        [XmlAttribute("turretZ")]
        public float TurretZ { get; set; }
        [XmlAttribute("sirens")]
        public bool Sirens { get; set; }
        [XmlAttribute("landingGearDown")]
        public bool LandingGearDown { get; set; }
        [XmlAttribute("locked")]
        public bool Locked { get; set; }
        [XmlAttribute("specialState")]
        public bool SpecialState { get; set; } // adjustable property
        [XmlAttribute("color")]
        public string Color { get; set; } = string.Empty;
        [XmlAttribute("headLightColor")]
        public string HeadLightColor { get; set; } = string.Empty;
        [XmlAttribute("paintjob")]
        public int Paintjob { get; set; }
        [XmlAttribute("upgrades")]
        public string Upgrades { get; set; } = string.Empty;
        [XmlAttribute("plate")]
        public string Plate { get; set; } = string.Empty;
        [XmlAttribute("dimension")]
        public short Dimension { get; set; }
        [XmlAttribute("interior")]
        public byte Interior { get; set; }
        [XmlAttribute("collisions")]
        public bool Collisions { get; set; }
        [XmlAttribute("alpha")]
        public bool Alpha { get; set; }
        [XmlAttribute("frozen")]
        public bool Frozen { get; set; }
        [XmlAttribute("taxiLightOn")]
        public bool TaxiLightOn { get; set; }
        [XmlAttribute("engineOn")]
        public bool TaxiLengineOnightOn { get; set; }
        [XmlAttribute("lightsOn")]
        public bool LightsOn { get; set; }
        [XmlAttribute("damageProof")]
        public bool DamageProof { get; set; }
        [XmlAttribute("explodableFuelTank")]
        public bool ExplodableFuelTank { get; set; }
        [XmlAttribute("toggleRespawn")]
        public bool ToggleRespawn { get; set; }
        [XmlAttribute("respawnDelay")]
        public int RespawnDelay { get; set; }
        [XmlAttribute("respawnPosX")]
        public float RespawnPosX { get; set; }
        [XmlAttribute("respawnPosY")]
        public float RespawnPosY { get; set; }
        [XmlAttribute("respawnPosZ")]
        public float RespawnRotX { get; set; }
        [XmlAttribute("respawnRotX")]
        public float RespawnRotY { get; set; }
        [XmlAttribute("respawnRotY")]
        public float RespawnRotZ { get; set; }
        [XmlAttribute("respawnRotZ")]
        public float respawnRotX { get; set; }

        [XmlIgnore]
        public VehicleModel ObjectModel => (VehicleModel)Enum.Parse(typeof(VehicleModel), this.Model.ToString());
    }
}
