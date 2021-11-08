using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Events
{
    public class ElementDataChangedArgs : EventArgs
    {
        public string DataName { get; set; }
        public LuaValue NewValue { get; set; }
        public LuaValue? OldValue { get; set; }
        public DataSyncType SyncType { get; set; }

        public ElementDataChangedArgs(string dataName, LuaValue newValue, LuaValue? oldValue = null, DataSyncType syncType = DataSyncType.Local)
        {
            this.DataName = dataName;
            this.NewValue = newValue;
            this.OldValue = oldValue;
            this.SyncType = syncType;
        }
    }
}
