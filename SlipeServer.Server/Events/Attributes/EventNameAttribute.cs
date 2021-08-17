using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Events.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventNameAttribute : Attribute
    {
        public string Name { get; }
        public EventNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
