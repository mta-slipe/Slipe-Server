using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using System;
using System.Linq;

namespace SlipeServer.Scripting.EventDefinitions;

public class ResourceEventDefinitions(IMtaServer server, IResourceService resourceService) : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<RootElement>(
            "onResourcePreStart",
            (callback) =>
            {
                void callbackProxy(Resource resource) => callback.CallbackDelegate(server.RootElement, resource);
                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) => resourceService.ResourceStarting += callbackProxy,
                    Remove = (_) => resourceService.ResourceStarting -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<RootElement>(
            "onResourceStart",
            (callback) =>
            {
                void callbackProxy(Resource resource) => callback.CallbackDelegate(resource.Root, resource);
                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) => resourceService.ResourceStarted += callbackProxy,
                    Remove = (_) => resourceService.ResourceStarted -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<RootElement>(
            "onResourceStop",
            (callback) =>
            {
                void callbackProxy(Resource resource) => callback.CallbackDelegate(resource.Root, resource, false);
                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) => resourceService.ResourceStopped += callbackProxy,
                    Remove = (_) => resourceService.ResourceStopped -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerResourceStart",
            (callback) =>
            {
                void callbackProxy(Player sender, SlipeServer.Server.Elements.Events.PlayerResourceStartedEventArgs e)
                {
                    var resource = resourceService.StartedResources.FirstOrDefault(r => r.NetId == e.NetId);
                    if (resource != null)
                        callback.CallbackDelegate(sender, resource);
                }

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.ResourceStarted += callbackProxy,
                    Remove = (element) => element.ResourceStarted -= callbackProxy
                };
            }
        );
    }
}
