using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class ObjectScriptDefinitions(IMtaServer server)
{
    [ScriptFunctionDefinition("createObject")]
    public WorldObject CreateObject(ushort model, Vector3 position, Vector3? rotation = null, bool isLowLod = false)
    {
        return new WorldObject(model, position)
        {
            Rotation = rotation ?? Vector3.Zero,
            IsLowLod = isLowLod
        }.AssociateWith(server);
    }
}
