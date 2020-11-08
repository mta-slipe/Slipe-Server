using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements
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
