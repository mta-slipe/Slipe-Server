namespace SlipeServer.Packets.Definitions.Entities.Structs;

public struct PedWeapon
{
    public byte Slot { get; set; }
    public byte Type { get; set; }
    public ushort Ammo { get; set; }
    public ushort AmmoInClip { get; set; }
}
