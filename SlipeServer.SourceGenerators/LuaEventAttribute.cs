using System;

namespace SlipeServer.SourceGenerators
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
    public sealed class LuaEventAttribute : Attribute
    {
        public LuaEventAttribute()
        {
        }
    }
}
