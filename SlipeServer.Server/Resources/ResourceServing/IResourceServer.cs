using SlipeServer.Packets.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Resources.ResourceServing
{
    public interface IResourceServer
    {
        void Start();
        void Stop();
        ushort AllocateNetId();
        void ReleaseNetId(ushort netId);

        IEnumerable<ResourceFile> GetResourceFiles();
        IEnumerable<ResourceFile> GetResourceFiles(string resource);
    }
}
