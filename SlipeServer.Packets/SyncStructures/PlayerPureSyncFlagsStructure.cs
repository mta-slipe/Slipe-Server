using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Structures;

public class PlayerPureSyncFlagsStructure : ISyncStructure
{
    public bool IsInWater { get; set; }
    public bool IsOnGround { get; set; }
    public bool HasJetpack { get; set; }
    public bool IsDucked { get; set; }
    public bool WearsGoggles { get; set; }
    public bool HasContact { get; set; }
    public bool IsChoking { get; set; }
    public bool AkimboTargetUp { get; set; }
    public bool IsOnFire { get; set; }
    public bool HasAWeapon { get; set; }
    public bool IsSyncingVelocity { get; set; }
    public bool IsStealthAiming { get; set; }


    public PlayerPureSyncFlagsStructure()
    {

    }

    public PlayerPureSyncFlagsStructure(
        bool isInWater,
        bool isOnGround,
        bool hasJetpack,
        bool isDucked,
        bool wearsGoggles,
        bool hasContact,
        bool isChoking,
        bool akimboTargetUp,
        bool isOnFire,
        bool hasAWeapon,
        bool isSyncingVelocity,
        bool isStealthAiming
    )
    {
        this.IsInWater = isInWater;
        this.IsOnGround = isOnGround;
        this.HasJetpack = hasJetpack;
        this.IsDucked = isDucked;
        this.WearsGoggles = wearsGoggles;
        this.HasContact = hasContact;
        this.IsChoking = isChoking;
        this.AkimboTargetUp = akimboTargetUp;
        this.IsOnFire = isOnFire;
        this.HasAWeapon = hasAWeapon;
        this.IsSyncingVelocity = isSyncingVelocity;
        this.IsStealthAiming = isStealthAiming;
    }

    public void Read(PacketReader reader)
    {
        this.AkimboTargetUp = reader.GetBit();
        this.IsChoking = reader.GetBit();
        this.HasContact = reader.GetBit();
        this.WearsGoggles = reader.GetBit();

        this.IsDucked = reader.GetBit();
        this.HasJetpack = reader.GetBit();
        this.IsOnGround = reader.GetBit();
        this.IsInWater = reader.GetBit();

        this.IsStealthAiming = reader.GetBit();
        this.IsSyncingVelocity = reader.GetBit();
        this.HasAWeapon = reader.GetBit();
        this.IsOnFire = reader.GetBit();
    }

    public void Write(PacketBuilder builder)
    {
        builder.Write(this.AkimboTargetUp);
        builder.Write(this.IsChoking);
        builder.Write(this.HasContact);
        builder.Write(this.WearsGoggles);

        builder.Write(this.IsDucked);
        builder.Write(this.HasJetpack);
        builder.Write(this.IsOnGround);
        builder.Write(this.IsInWater);

        builder.Write(this.IsStealthAiming);
        builder.Write(this.IsSyncingVelocity);
        builder.Write(this.HasAWeapon);
        builder.Write(this.IsOnFire);
    }
}
