using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Structures;

public class KeySyncFlagsStructure : ISyncStructure
{
    public bool IsDucked { get; set; }
    public bool IsChoking { get; set; }
    public bool AkimboTargetUp { get; set; }
    public bool IsSyncingVehicle { get; set; }


    public KeySyncFlagsStructure()
    {

    }

    public KeySyncFlagsStructure(
        bool isDucked,
        bool isChoking,
        bool akimboTargetUp,
        bool isSyncingVehicle
    )
    {
        this.IsDucked = isDucked;
        this.IsChoking = isChoking;
        this.AkimboTargetUp = akimboTargetUp;
        this.IsSyncingVehicle = isSyncingVehicle;
    }

    public void Read(PacketReader reader)
    {
        this.IsSyncingVehicle = reader.GetBit();
        this.AkimboTargetUp = reader.GetBit();
        this.IsChoking = reader.GetBit();
        this.IsDucked = reader.GetBit();
    }

    public void Write(PacketBuilder builder)
    {
        builder.Write(this.IsSyncingVehicle);
        builder.Write(this.AkimboTargetUp);
        builder.Write(this.IsChoking);
        builder.Write(this.IsDucked);
    }
}
