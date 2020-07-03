using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements
{
    public enum MarkerType
    {
        Checkpoint,
        Ring,
        Cylinder,
        Arrow,
        Corona,
        Invalid = 0xFF,
    }
}
