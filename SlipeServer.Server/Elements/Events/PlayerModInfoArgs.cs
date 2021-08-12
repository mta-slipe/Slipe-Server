using SlipeServer.Packets.Definitions.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Events
{
    public class PlayerModInfoArgs : EventArgs
    {
        public PlayerModInfoArgs(string infoType, IEnumerable<ModInfoItem> modInfoItems)
        {
            InfoType = infoType;
            ModInfoItems = modInfoItems;
        }

        public string InfoType { get; set; }
        public IEnumerable<ModInfoItem> ModInfoItems { get; set; }
    }
}
