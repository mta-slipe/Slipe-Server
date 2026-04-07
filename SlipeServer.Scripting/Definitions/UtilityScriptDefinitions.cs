using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace SlipeServer.Scripting.Definitions;

public readonly record struct PerformanceStatsResult(LuaValue Columns, LuaValue Rows);

public class UtilityScriptDefinitions(
    IMtaServer server,
    IGameWorld gameWorld,
    IDevelopmentModeService developmentModeService,
    ITransferBoxService transferBoxService,
    IScriptRefService scriptRefService,
    IDebugLog debugLog)
{

    private static LuaValue LuaTable(params (string Key, object? Value)[] entries)
    {
        var dict = new Dictionary<LuaValue, LuaValue>();
        foreach (var (key, val) in entries)
            dict[new LuaValue(key)] = ToLuaValue(val);
        return new LuaValue(dict);
    }

    private static LuaValue ToLuaValue(object? value) => value switch
    {
        null => LuaValue.Nil,
        LuaValue lv => lv,
        bool b => new LuaValue(b),
        string s => new LuaValue(s),
        int i => new LuaValue(i),
        long l => new LuaValue((double?)l),
        double d => new LuaValue(d),
        float f => new LuaValue((double?)f),
        _ => LuaValue.Nil
    };

    [ScriptFunctionDefinition("getTickCount")]
    public double GetTickCount()
    {
        return Math.Floor(server.Uptime.TotalMilliseconds + 0.5);
    }

    [ScriptFunctionDefinition("getRealTime")]
    public LuaValue GetRealTime(long? seconds = null, bool localTime = true)
    {
        DateTimeOffset dt = seconds.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(seconds.Value)
            : DateTimeOffset.UtcNow;

        DateTime t = localTime ? dt.LocalDateTime : dt.UtcDateTime;
        long ts = seconds ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return LuaTable(
            ("second", t.Second),
            ("minute", t.Minute),
            ("hour", t.Hour),
            ("monthday", t.Day),
            ("month", t.Month - 1),
            ("year", t.Year - 1900),
            ("weekday", (int)t.DayOfWeek),
            ("yearday", t.DayOfYear - 1),
            ("isdst", t.IsDaylightSavingTime()),
            ("timestamp", (long)ts)
        );
    }


    [ScriptFunctionDefinition("base64Encode")]
    public string Base64Encode(string data)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    }

    [ScriptFunctionDefinition("base64Decode")]
    public string Base64Decode(string data)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(data));
    }

    
    [ScriptFunctionDefinition("tocolor")]
    public int ToColor(byte r, byte g, byte b, byte a = 255)
    {
        return b + g * 256 + r * 256 * 256 + a * 256 * 256 * 256;
    }

    [ScriptFunctionDefinition("getColorFromString")]
    public Color? GetColorFromString(string color)
    {
        try
        {
            return ColorTranslator.FromHtml(color);
        }
        catch (Exception)
        {
            return null;
        }
    }


    [ScriptFunctionDefinition("md5")]
    public string CreateMD5(string input)
    {
        byte[] hashBytes = MD5.HashData(Encoding.ASCII.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
    }

    [ScriptFunctionDefinition("sha256")]
    public string Sha256(string input)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
    }


    [ScriptFunctionDefinition("getDevelopmentMode")]
    public bool GetDevelopmentMode() => developmentModeService.IsEnabled;

    [ScriptFunctionDefinition("setDevelopmentMode")]
    public bool SetDevelopmentMode(bool enable, bool enableWeb = false)
    {
        developmentModeService.IsEnabled = enable;
        return true;
    }


    [ScriptFunctionDefinition("getFPSLimit")]
    public int GetFPSLimit() => gameWorld.FpsLimit;

    [ScriptFunctionDefinition("setFPSLimit")]
    public bool SetFPSLimit(int limit)
    {
        if (limit < 25 || limit > 100)
            return false;
        gameWorld.FpsLimit = (byte)limit;
        return true;
    }


    [ScriptFunctionDefinition("isTransferBoxVisible")]
    public bool IsTransferBoxVisible() => transferBoxService.IsVisible;

    [ScriptFunctionDefinition("setTransferBoxVisible")]
    public bool SetTransferBoxVisible(bool visible)
    {
        return transferBoxService.SetVisible(visible);
    }


    [ScriptFunctionDefinition("isOOPEnabled")]
    public bool IsOOPEnabled() => false;

    [ScriptFunctionDefinition("getVersion")]
    public LuaValue GetVersion()
    {
        return LuaTable(
            ("number", 0x1_05_00),
            ("mta", "1.6.0"),
            ("name", "Slipe Server"),
            ("netcode", 0),
            ("os", Environment.OSVersion.Platform.ToString()),
            ("type", "server"),
            ("tag", string.Empty),
            ("sortable", "1.6.0")
        );
    }

    [ScriptFunctionDefinition("getUserdataType")]
    public string GetUserdataType(object? value)
    {
        return value?.GetType().Name ?? "nil";
    }


    [ScriptFunctionDefinition("getNetworkStats")]
    public LuaValue GetNetworkStats()
    {
        return LuaTable(
            ("bytesReceived", 0),
            ("bytesSent", 0),
            ("packetsReceived", 0),
            ("packetsSent", 0),
            ("packetlossTotal", 0.0),
            ("packetlossLastSecond", 0.0),
            ("messagesInSendBuffer", 0),
            ("messagesInResendBuffer", 0),
            ("isLimitedByCongestionControl", false),
            ("isLimitedByOutgoingBandwidthLimit", false),
            ("encryptionStatus", 0)
        );
    }

    [ScriptFunctionDefinition("getNetworkUsageData")]
    public LuaValue GetNetworkUsageData()
    {
        var empty = new LuaValue(new Dictionary<LuaValue, LuaValue>());
        return LuaTable(
            ("in", LuaTable(("bits", empty), ("count", empty))),
            ("out", LuaTable(("bits", empty), ("count", empty)))
        );
    }

    [ScriptFunctionDefinition("getPerformanceStats")]
    public PerformanceStatsResult GetPerformanceStats(string category, string options = "", string filter = "")
    {
        var empty = new LuaValue(new Dictionary<LuaValue, LuaValue>());
        return new PerformanceStatsResult(empty, empty);
    }

    [ScriptFunctionDefinition("getProcessMemoryStats")]
    public LuaValue GetProcessMemoryStats()
    {
        var proc = Process.GetCurrentProcess();
        return LuaTable(
            ("virtualMemorySize", (long)proc.VirtualMemorySize64),
            ("residentMemorySize", (long)proc.WorkingSet64),
            ("privateMemorySize", (long)proc.PrivateMemorySize64)
        );
    }


    [ScriptFunctionDefinition("debugSleep")]
    public bool DebugSleep(int sleep)
    {
        if (!developmentModeService.IsEnabled)
            return false;

        Thread.Sleep(sleep);

        return true;
    }

    [ScriptFunctionDefinition("addDebugHook")]
    public bool AddDebugHook(string hookType, ScriptCallbackDelegateWrapper callback, IEnumerable<string>? nameList = null)
    {
        return true;
    }

    [ScriptFunctionDefinition("removeDebugHook")]
    public bool RemoveDebugHook(string hookType, ScriptCallbackDelegateWrapper callback)
    {
        return true;
    }


    [ScriptFunctionDefinition("inspect")]
    public string Inspect(LuaValue value, LuaValue? options = null)
    {
        return JsonSerializer.Serialize(LuaValueToObject(value), new JsonSerializerOptions { WriteIndented = true });
    }

    [ScriptFunctionDefinition("iprint")]
    public bool Iprint(params LuaValue[] values)
    {
        foreach (var value in values)
            debugLog.Output(JsonSerializer.Serialize(LuaValueToObject(value)));

        return true;
    }


    [ScriptFunctionDefinition("ref")]
    public int Ref(LuaValue value)
    {
        return scriptRefService.Ref(value);
    }

    [ScriptFunctionDefinition("deref")]
    public LuaValue Deref(int reference)
    {
        return scriptRefService.Deref(reference);
    }

    private static object? LuaValueToObject(LuaValue value)
    {
        if (value.IsNil) 
            return null;

        if (value.BoolValue.HasValue) 
            return value.BoolValue.Value;

        if (value.StringValue != null) 
            return value.StringValue;

        if (value.IntegerValue.HasValue) 
            return value.IntegerValue.Value;

        if (value.FloatValue.HasValue) 
            return value.FloatValue.Value;

        if (value.DoubleValue.HasValue) 
            return value.DoubleValue.Value;

        if (value.TableValue != null)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var kvp in value.TableValue)
                dict[kvp.Key.StringValue ?? kvp.Key.IntegerValue?.ToString() ?? ""] = LuaValueToObject(kvp.Value);

            return dict;
        }

        return null;
    }
}
