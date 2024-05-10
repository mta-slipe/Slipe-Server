using System;
using System.Collections.Generic;

namespace SlipeServer.Packets.Definitions.Entities.Structs;

[Obsolete("It is highly not recommended to use element data!")]
public class CustomData
{
    public IEnumerable<CustomDataItem> Items { get; set; } = Array.Empty<CustomDataItem>();

    public readonly static CustomData Empty = new();
}
