using SlipeServer.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Console.LuaDefinitions.Userdatas
{
    public class FooUserdata : IUserdata
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}
