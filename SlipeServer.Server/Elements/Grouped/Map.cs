using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Grouped
{
    public class Map : Dictionary<string, object>
    {
        public void AssociateWith(MtaServer server)
        {
            foreach (var pair in this)
            {
                if (pair.Value is Element element)
                    server.AssociateElement(element);
            }
        }
    }
}
