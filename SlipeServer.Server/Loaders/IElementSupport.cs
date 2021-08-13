using SlipeServer.Server.Elements;
using SlipeServer.Server.Loaders.Map;
using SlipeServer.Server.Loaders.Map.ElementsDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loaders
{
    public interface IElementSupport
    {
        public string GetNodeName();
        public Type GetNodeType();
    }
}
