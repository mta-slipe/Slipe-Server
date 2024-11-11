using SlipeServer.Server;
using SlipeServer.Server.Resources;

namespace SlipeServer.Example;

internal class NaviagationResource : Resource
{
    public NaviagationResource(MtaServer server) : base(server, server.RootElement, "Navigation")
    {
        this.NoClientScripts["navigation.lua"] = System.Text.UTF8Encoding.UTF8.GetBytes("""
            local points = {}

            addEventHandler("onClientRender", root, function()
                for i=1, #points-1 do
                    local current = points[i];
                    local next = points[i + 1];
                    dxDrawLine3D ( current[1], current[2], current[3], next[1], next[2], next[3], tocolor ( 0, 255, 0, 230 ), 8)
                end
            end)
            addEvent("showNavigation", true)
            addEventHandler("showNavigation", root, function(newPoints)
                points = newPoints;
            end);
            """);
    }
}
