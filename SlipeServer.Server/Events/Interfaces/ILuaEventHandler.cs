using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Events.Interfaces
{
    public interface ILuaEventHandler
    {
        public string BaseName { get; }
    }
}
