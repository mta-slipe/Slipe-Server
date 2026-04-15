using SlipeServer.Lua;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Scripting;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace SlipeServer.DropInReplacement.MixedResources.Behaviour;

public class DropInReplacementResourceLuaService : IDropInReplacementResourceLuaService
{
    private readonly LuaService luaService;
    private readonly ISettingsRegistry settingsRegistry;

    public DropInReplacementResourceLuaService(LuaService luaService, IScriptEventRuntime scriptEventRuntime, ISettingsRegistry settingsRegistry)
    {
        this.luaService = luaService;
        this.settingsRegistry = settingsRegistry;

        luaService.LoadDefaultDefinitions();
        scriptEventRuntime.LoadDefaultEvents();
    }

    public void StartLuaResource(MixedResource resource)
    {
        foreach (var (name, value) in resource.Settings)
            this.settingsRegistry.Set($"{resource.Name}.{name}", ParseSettingValue(value));

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

    private static LuaValue ParseSettingValue(string raw)
    {
        raw = raw.Trim();

        if (raw.StartsWith('[') || raw.StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                return JsonElementToLuaValue(doc.RootElement);
            }
            catch { }
        }

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            return new LuaValue(number);
        if (bool.TryParse(raw, out var boolean))
            return new LuaValue(boolean);

        return new LuaValue(raw);
    }

    private static LuaValue JsonElementToLuaValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            var items = element.EnumerateArray().ToList();
            // Single-element primitive arrays are the scalar value (MTA meta.xml convention)
            if (items.Count == 1 && items[0].ValueKind is JsonValueKind.Number or JsonValueKind.String or JsonValueKind.True or JsonValueKind.False)
                return JsonElementToLuaValue(items[0]);

            var table = new Dictionary<LuaValue, LuaValue>();
            for (int i = 0; i < items.Count; i++)
                table[new LuaValue(i + 1)] = JsonElementToLuaValue(items[i]);
            return new LuaValue(table);
        }

        return element.ValueKind switch
        {
            JsonValueKind.Number => new LuaValue(element.GetDouble()),
            JsonValueKind.String => new LuaValue(element.GetString()),
            JsonValueKind.True => new LuaValue(true),
            JsonValueKind.False => new LuaValue(false),
            _ => LuaValue.Nil
        };
    }
}

public interface IDropInReplacementResourceLuaService
{
    void StartLuaResource(MixedResource resource);
    void StopLuaResource(MixedResource resource);
}
