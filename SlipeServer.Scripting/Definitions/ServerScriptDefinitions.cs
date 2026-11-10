using SlipeServer.Packets.Definitions.Lua.Rpc.World;
using SlipeServer.Server;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using System;
using System.Collections.Generic;

namespace SlipeServer.Scripting.Definitions;

public class ServerScriptDefinitions(IMtaServer server)
{
    private static readonly Dictionary<string, GlitchType> glitchNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["quickreload"] = GlitchType.GLITCH_QUICKRELOAD,
        ["fastfire"] = GlitchType.GLITCH_FASTFIRE,
        ["fastmove"] = GlitchType.GLITCH_FASTMOVE,
        ["crouchbug"] = GlitchType.GLITCH_CROUCHBUG,
        ["highcloserangedamage"] = GlitchType.GLITCH_CLOSEDAMAGE,
        ["hitanim"] = GlitchType.GLITCH_HITANIM,
        ["fastsprint"] = GlitchType.GLITCH_FASTSPRINT,
        ["baddrivebyhitbox"] = GlitchType.GLITCH_BADDRIVEBYHITBOX,
        ["quickstand"] = GlitchType.GLITCH_QUICKSTAND,
        ["kickoutofvehicle_onmodelreplace"] = GlitchType.GLITCH_KICKOUTOFVEHICLE_ONMODELREPLACE,
        ["vehicle_rapid_stop"] = GlitchType.GLITCH_VEHICLE_RAPID_STOP,
    };

    private readonly Dictionary<GlitchType, bool> glitchStates = [];

    [ScriptFunctionDefinition("getMaxPlayers")]
    public int GetMaxPlayers() => server.Configuration.MaxPlayerCount;

    [ScriptFunctionDefinition("setMaxPlayers")]
    public bool SetMaxPlayers(int slots)
    {
        server.SetMaxPlayers((ushort)slots);
        return true;
    }

    [ScriptFunctionDefinition("getServerName")]
    public string GetServerName() => server.Configuration.ServerName;

    [ScriptFunctionDefinition("getServerPassword")]
    public string? GetServerPassword() => server.Password;

    [ScriptFunctionDefinition("setServerPassword")]
    public bool SetServerPassword(string? password)
    {
        server.Password = password;
        return true;
    }

    [ScriptFunctionDefinition("getServerPort")]
    public int GetServerPort() => server.Configuration.Port;

    [ScriptFunctionDefinition("getServerHttpPort")]
    public int GetServerHttpPort() => server.Configuration.HttpPort;

    [ScriptFunctionDefinition("getServerIpFromMasterServer")]
    public string GetServerIpFromMasterServer() => server.Configuration.MasterServerHost;

    [ScriptFunctionDefinition("getServerConfigSetting")]
    public string? GetServerConfigSetting(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "servername" => server.Configuration.ServerName,
            "serverip" or "host" => server.Configuration.Host,
            "serverport" or "port" => server.Configuration.Port.ToString(),
            "maxplayers" => server.Configuration.MaxPlayerCount.ToString(),
            "password" => server.Password,
            "httpport" => server.Configuration.HttpPort.ToString(),
            "httphost" => server.Configuration.HttpHost,
            "httpurl" => server.Configuration.HttpUrl,
            "minclientversion" => server.Configuration.MinVersion,
            _ => null,
        };
    }

    [ScriptFunctionDefinition("setServerConfigSetting")]
    public bool SetServerConfigSetting(string name, string value, bool save = false)
    {
        switch (name.ToLowerInvariant())
        {
            case "servername":
                server.Configuration.ServerName = value;
                break;
            case "serverip" or "host":
                server.Configuration.Host = value;
                break;
            case "serverport" or "port":
                if (ushort.TryParse(value, out var port))
                    server.Configuration.Port = port;
                else
                    return false;
                break;
            case "maxplayers":
                if (ushort.TryParse(value, out var maxPlayers))
                    server.SetMaxPlayers(maxPlayers);
                else
                    return false;
                break;
            case "password":
                server.Password = string.IsNullOrEmpty(value) ? null : value;
                break;
            case "httpport":
                if (ushort.TryParse(value, out var httpPort))
                    server.Configuration.HttpPort = httpPort;
                else
                    return false;
                break;
            case "minclientversion":
                server.Configuration.MinVersion = value;
                break;
            default:
                return false;
        }
        return true;
    }

    [ScriptFunctionDefinition("isGlitchEnabled")]
    public bool IsGlitchEnabled(string glitchName)
    {
        if (!glitchNameMap.TryGetValue(glitchName, out var glitchType))
            return false;

        return this.glitchStates.TryGetValue(glitchType, out var enabled) && enabled;
    }

    [ScriptFunctionDefinition("setGlitchEnabled")]
    public bool SetGlitchEnabled(string glitchName, bool enabled)
    {
        if (!glitchNameMap.TryGetValue(glitchName, out var glitchType))
            return false;

        this.glitchStates[glitchType] = enabled;

        var packet = new SetGlitchEnabledPacket((byte)glitchType, enabled);
        packet.SendTo(server.Players);

        return true;
    }

    [ScriptFunctionDefinition("shutdown")]
    public bool Shutdown(string reason = "No reason specified", int exitCode = 0)
    {
        server.Stop();
        return true;
    }
}
