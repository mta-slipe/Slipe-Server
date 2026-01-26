using System.Reflection;

namespace SlipeServer.Console.AdditionalResources.Parachute;
public static class ResourceFiles
{
    public static byte[] UtilityLua { get; } = GetLuaFile("SlipeServer.Console.AdditionalResources.Parachute.Lua.utility.lua");
    public static byte[] ParachuteClientLua { get; } = GetLuaFile("SlipeServer.Console.AdditionalResources.Parachute.Lua.parachute_cl.lua");
    public static byte[] OpenChuteLua { get; } = GetLuaFile("SlipeServer.Console.AdditionalResources.Parachute.Lua.openChute.lua");
    public static byte[] SkydivingClientLua { get; } = GetLuaFile("SlipeServer.Console.AdditionalResources.Parachute.Lua.skydiving_cl.lua");
    public static byte[] ClientAnimationLua { get; } = GetLuaFile("SlipeServer.Console.AdditionalResources.Parachute.Lua.client_anim.lua");
    public static byte[] ParachuteOpenMp3 { get; } = GetLuaFile("SlipeServer.Console.AdditionalResources.Parachute.Lua.parachuteopen.mp3");

    private static byte[] GetLuaFile(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(name);

        if (stream == null)
            throw new FileNotFoundException($"File \"{name}\" not found in embedded resources.");

        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        return buffer;
    }
}
