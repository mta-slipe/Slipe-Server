using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions
{
    [ScriptFunctionDefinition("removeWorldModel")]
    public bool RemoveWorldModel(int modelId, float radius, float x, float y, float z, int interior = 0)
    {
        gameWorld.RemoveWorldModel((ushort)modelId, new Vector3(x, y, z), radius, (byte)interior);
        return true;
    }

    [ScriptFunctionDefinition("restoreWorldModel")]
    public bool RestoreWorldModel(int modelId, float radius, float x, float y, float z, int interior = 0)
    {
        gameWorld.RestoreWorldModel((ushort)modelId, new Vector3(x, y, z), radius, (byte)interior);
        return true;
    }

    [ScriptFunctionDefinition("restoreAllWorldModels")]
    public bool RestoreAllWorldModels()
    {
        gameWorld.RestoreAllWorldModels();
        return true;
    }
}
