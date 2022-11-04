using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class RadarAreaScriptDefinitions
{
    private readonly MtaServer server;

    public RadarAreaScriptDefinitions(MtaServer server)
    {
        this.server = server;
    }

    [ScriptFunctionDefinition("createRadarArea")]
    public RadarArea CreateRadarArea(Vector2 position, Vector2 size, Color? color = null, Player? visibleFor = null)
    {
        var radarArea = new RadarArea(position, size, color ?? Color.FromArgb(255, 0, 0, 255)).AssociateWith(this.server);
        return radarArea;
    }

    [ScriptFunctionDefinition("getRadarAreaColor")]
    public Color GetRadarAreaColor(RadarArea radarArea) => radarArea.Color;

    [ScriptFunctionDefinition("getRadarAreaSize")]
    public Vector2 GetRadarAreaSize(RadarArea radarArea) => radarArea.Size;

    [ScriptFunctionDefinition("isInsideRadarArea")]
    public bool IsInsideRadarArea(RadarArea radarArea, Vector2 position) => radarArea.IsInside(position);

    [ScriptFunctionDefinition("isRadarAreaFlashing")]
    public bool IsRadarAreaFlashing(RadarArea radarArea) => radarArea.IsFlashing;

    [ScriptFunctionDefinition("setRadarAreaColor")]
    public void SetRadarAreaColor(RadarArea radarArea, Color color)
    {
        radarArea.Color = color;
    }

    [ScriptFunctionDefinition("setRadarAreaFlashing")]
    public void SetRadarAreaFlashing(RadarArea radarArea, bool flashing)
    {
        radarArea.IsFlashing = flashing;
    }

    [ScriptFunctionDefinition("setRadarAreaSize")]
    public void SetRadarAreaSize(RadarArea radarArea, Vector2 size)
    {
        radarArea.Size = size;
    }
}
