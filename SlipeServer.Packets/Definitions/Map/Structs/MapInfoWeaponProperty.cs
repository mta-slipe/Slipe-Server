using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Map.Structs
{
    public struct MapInfoWeaponProperty
    {
        public byte WeaponType { get; set; }
        public bool EnabledWhenUsingJetpack { get; set; }
        public MapInfoWeaponConfiguration[] WeaponConfigurations { get; set; }
    }
}
