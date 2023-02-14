using System;
using System.Collections.Generic;

namespace SlipeServer.Packets.Definitions.Entities.Structs;

public class CustomData
{
    public IEnumerable<CustomDataItem> Items { get; set; } = Array.Empty<CustomDataItem>();

}
