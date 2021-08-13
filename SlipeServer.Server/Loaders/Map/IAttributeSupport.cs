using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loaders.Map
{
    public interface IAttributeSupport
    {
        public string GetAttributeName();
        public object Parse(string value);
    }
}
