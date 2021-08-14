using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Entities.Structs
{
    public class CustomData
    {
        public CustomDataItem[] Items { get; set; } = Array.Empty<CustomDataItem>();

    }
}
