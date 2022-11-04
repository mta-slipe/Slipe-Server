using System;

namespace SlipeServer.Packets.Definitions.Entities.Structs;

public class CustomData
{
    public CustomDataItem[] Items { get; set; } = Array.Empty<CustomDataItem>();

}
