using SlipeServer.Lua;
using SlipeServer.Scripting;
using System.Text;

namespace SlipeServer.DropInReplacement.MixedResources.Behaviour;

public class DropInReplacementResourceLuaService : IDropInReplacementResourceLuaService
{
    private readonly LuaService luaService;

    public DropInReplacementResourceLuaService(LuaService luaService, IScriptEventRuntime scriptEventRuntime)
    {
        this.luaService = luaService;

        luaService.LoadDefaultDefinitions();
        scriptEventRuntime.LoadDefaultEvents();
    }

    public void StartLuaResource(MixedResource resource)
    {
        var environment = this.luaService.CreateEnvironment(resource.Name, resource);
        foreach (var file in resource.ServerFiles.Where(x => x.FileType == Server.Elements.Enums.ResourceFileType.Script))
        {
            environment.LoadString(Encoding.UTF8.GetString(file.Content), $"{resource.Name}/{file.Name}");
        }
    }

    public void StopLuaResource(MixedResource resource)
    {
        this.luaService.UnloadScriptsFor(resource);
    }

}

public interface IDropInReplacementResourceLuaService
{
    void StartLuaResource(MixedResource resource);
    void StopLuaResource(MixedResource resource);
}
