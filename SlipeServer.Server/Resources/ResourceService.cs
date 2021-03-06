using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.ResourceServing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipeServer.Server.Resources
{
    public class ResourceService
    {
        private readonly MtaServer server;
        private readonly RootElement root;
        private readonly IResourceServer resourceServer;

        private readonly List<Resource> startedResources;

        public IReadOnlyCollection<Resource> StartedResources => startedResources.AsReadOnly();

        public ResourceService(MtaServer server, RootElement root, IResourceServer resourceServer)
        {
            this.server = server;
            this.root = root;
            this.resourceServer = resourceServer;

            this.startedResources = new List<Resource>();

            this.server.PlayerJoined += HandlePlayerJoin;
        }

        private void HandlePlayerJoin(Player player)
        {
            foreach (var resource in this.startedResources)
            {
                resource.StartFor(player);
            }
        }

        public void StartResource(string name)
        {
            if (!this.startedResources.Any(r => r.Name == name))
            {
                var resource = new Resource(this.server, this.root, this.resourceServer, name);
                resource.Start();
                this.startedResources.Add(resource);
            }
        }

        public void StopResource(string name)
        {
            var resource = this.startedResources.Single(r => r.Name == name);
            this.startedResources.Remove(resource);
            resource.Stop();
        }
    }
}
