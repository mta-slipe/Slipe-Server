using System.IO;
using System.Reflection;

namespace SlipeServer.Console.AdditionalResources.ResourceWithFeatures;

public static class ResourceFiles
{
    public static byte[] TestLua { get; } = GetLuaFile("SlipeServer.Console.AdditionalResources.ResourceWithFeatures.Lua.Test.lua");

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
