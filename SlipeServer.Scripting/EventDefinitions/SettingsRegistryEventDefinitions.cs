using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Scripting.Definitions;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System.Linq;
using System.Text.Json;

namespace SlipeServer.Scripting.EventDefinitions;

public class SettingsRegistryEventDefinitions(IMtaServer server, ISettingsRegistry settingsRegistry) : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<RootElement>(
            "onSettingChange",
            (callback) =>
            {
                void callbackProxy(object? sender, SettingChangedEventArgs e)
                    => callback.CallbackDelegate(
                        server.RootElement,
                        e.Setting,
                        JsonSerializer.Serialize(LuaValueToJsonCompatible(e.OldValue)),
                        JsonSerializer.Serialize(LuaValueToJsonCompatible(e.NewValue)));

                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) => settingsRegistry.SettingChanged += callbackProxy,
                    Remove = (_) => settingsRegistry.SettingChanged -= callbackProxy
                };
            }
        );
    }

    private static object? LuaValueToJsonCompatible(LuaValue value)
    {
        if (value.IsNil) 
            return null;
        if (value.BoolValue.HasValue) 
            return value.BoolValue.Value;
        if (value.StringValue != null)
            return value.StringValue;
        if (value.IntegerValue.HasValue) 
            return (double)value.IntegerValue.Value;
        if (value.FloatValue.HasValue)
            return (double)value.FloatValue.Value;
        if (value.DoubleValue.HasValue) 
            return value.DoubleValue.Value;
        if (value.TableValue != null)
            return value.TableValue.ToDictionary(
                kvp => kvp.Key.ToString()!,
                kvp => LuaValueToJsonCompatible(kvp.Value));

        return null;
    }
}
