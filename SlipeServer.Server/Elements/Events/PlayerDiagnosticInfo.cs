using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Events;

public class PlayerDiagnosticInfo : EventArgs
{
    public uint Level { get; set; }
    public string Message { get; set; }

    public PlayerDiagnosticInfo(uint level, string message)
    {
        this.Level = level;
        this.Message = message;
    }
}
