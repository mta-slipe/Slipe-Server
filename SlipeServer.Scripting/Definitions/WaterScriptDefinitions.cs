using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using SlipeServer.Server.Structs;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class WaterScriptDefinitions(IGameWorld gameWorld, IMtaServer server)
{
    [ScriptFunctionDefinition("createWater")]
    public Water CreateWater(
        float x1, float y1, float z1,
        float x2, float y2, float z2,
        float x3, float y3, float z3,
        float? x4 = null, float? y4 = null, float? z4 = null,
        bool shallow = false)
    {
        Vector3[] vertices;
        if (x4.HasValue && y4.HasValue && z4.HasValue)
        {
            vertices =
            [
                new Vector3(x1, y1, z1),
                new Vector3(x2, y2, z2),
                new Vector3(x3, y3, z3),
                new Vector3(x4.Value, y4.Value, z4.Value)
            ];
        }
        else
        {
            vertices =
            [
                new Vector3(x1, y1, z1),
                new Vector3(x2, y2, z2),
                new Vector3(x3, y3, z3)
            ];
        }

        var water = new Water(vertices)
        {
            IsShallow = shallow
        }.AssociateWith(server);

        if (ScriptExecutionContext.Current?.Owner != null)
            water.Parent = ScriptExecutionContext.Current.Owner?.DynamicRoot;

        return water;
    }

    [ScriptFunctionDefinition("getWaterColor")]
    public Color GetWaterColor()
        => gameWorld.WaterColor ?? Color.FromArgb(200, 0, 128, 255);

    [ScriptFunctionDefinition("getWaterVertexPosition")]
    public Vector3 GetWaterVertexPosition(Water water, int vertexIndex)
    {
        var vertices = water.Vertices.ToArray();
        if (vertexIndex < 1 || vertexIndex > vertices.Length)
            throw new ArgumentOutOfRangeException(nameof(vertexIndex), $"Vertex index must be between 1 and {vertices.Length}");
        return vertices[vertexIndex - 1];
    }

    [ScriptFunctionDefinition("getWaveHeight")]
    public float GetWaveHeight() => gameWorld.WaveHeight;

    [ScriptFunctionDefinition("resetWaterColor")]
    public bool ResetWaterColor()
    {
        gameWorld.WaterColor = null;
        return true;
    }

    [ScriptFunctionDefinition("resetWaterLevel")]
    public bool ResetWaterLevel()
    {
        gameWorld.WaterLevels = new WaterLevels();
        return true;
    }

    [ScriptFunctionDefinition("setWaterColor")]
    public bool SetWaterColor(int red, int green, int blue, int alpha = 200)
    {
        gameWorld.WaterColor = Color.FromArgb(alpha, red, green, blue);
        return true;
    }

    [ScriptFunctionDefinition("setWaterLevel")]
    public bool SetWaterLevel(
        float level,
        bool includeWaterFeatures = true,
        bool includeWaterElements = true,
        bool includeWorldSea = true,
        bool includeOutsideWorldSea = false)
    {
        gameWorld.WaterLevels = new WaterLevels
        {
            SeaLevel = includeWorldSea ? level : gameWorld.WaterLevels.SeaLevel,
            OutsideSeaLevel = includeOutsideWorldSea ? level : gameWorld.WaterLevels.OutsideSeaLevel,
            NonSeaLevel = includeWaterFeatures ? level : gameWorld.WaterLevels.NonSeaLevel,
        };
        return true;
    }

    [ScriptFunctionDefinition("setWaterVertexPosition")]
    public bool SetWaterVertexPosition(Water water, int vertexIndex, int x, int y, float z)
    {
        var vertices = water.Vertices.ToArray();
        if (vertexIndex < 1 || vertexIndex > vertices.Length)
            return false;

        vertices[vertexIndex - 1] = new Vector3(x, y, z);
        water.Vertices = vertices;
        return true;
    }

    [ScriptFunctionDefinition("setWaveHeight")]
    public bool SetWaveHeight(float height)
    {
        gameWorld.WaveHeight = height;
        return true;
    }
}
