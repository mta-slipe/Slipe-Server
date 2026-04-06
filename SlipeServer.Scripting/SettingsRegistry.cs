using SlipeServer.Packets.Definitions.Lua;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SlipeServer.Scripting;

public interface ISettingsRegistry
{
    LuaValue? Get(string settingName);
    void Set(string settingName, LuaValue value);
}

public class SettingsRegistry : ISettingsRegistry
{
    private readonly Dictionary<string, LuaValue> settings = [];
    private readonly string settingsFilePath;

    public SettingsRegistry()
    {
        this.settingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
        LoadFromFile();
    }

    public LuaValue? Get(string settingName)
    {
        this.settings.TryGetValue(settingName, out var value);
        return value;
    }

    public void Set(string settingName, LuaValue value)
    {
        this.settings[settingName] = value;
        SaveToFile();
    }

    private void LoadFromFile()
    {
        if (!File.Exists(this.settingsFilePath))
            return;

        try
        {
            var json = File.ReadAllText(this.settingsFilePath);
            using var doc = JsonDocument.Parse(json);
            foreach (var prop in doc.RootElement.EnumerateObject())
                this.settings[prop.Name] = JsonElementToLuaValue(prop.Value);
        }
        catch { }
    }

    private void SaveToFile()
    {
        try
        {
            var data = this.settings.ToDictionary(
                kvp => kvp.Key,
                kvp => LuaValueToJsonCompatible(kvp.Value));

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(this.settingsFilePath, json);
        }
        catch { }
    }

    private static object? LuaValueToJsonCompatible(LuaValue value)
    {
        if (value.IsNil) return null;
        if (value.BoolValue.HasValue) return value.BoolValue.Value;
        if (value.StringValue != null) return value.StringValue;
        if (value.IntegerValue.HasValue) return (double)value.IntegerValue.Value;
        if (value.FloatValue.HasValue) return (double)value.FloatValue.Value;
        if (value.DoubleValue.HasValue) return value.DoubleValue.Value;
        if (value.TableValue != null)
            return value.TableValue.ToDictionary(
                kvp => kvp.Key.ToString()!,
                kvp => LuaValueToJsonCompatible(kvp.Value));
        return null;
    }

    private static LuaValue JsonElementToLuaValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => LuaValue.Nil,
            JsonValueKind.True => new LuaValue(true),
            JsonValueKind.False => new LuaValue(false),
            JsonValueKind.String => new LuaValue(element.GetString()),
            JsonValueKind.Number => new LuaValue(element.GetDouble()),
            JsonValueKind.Object => new LuaValue(element.EnumerateObject()
                .ToDictionary(
                    p => new LuaValue(p.Name),
                    p => JsonElementToLuaValue(p.Value))),
            JsonValueKind.Array => new LuaValue(element.EnumerateArray()
                .Select(JsonElementToLuaValue)
                .ToList()),
            _ => LuaValue.Nil,
        };
    }
}
