using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;
/// <summary>
/// https://wiki.multitheftauto.com/wiki/Server_Scripting_Functions#Blip_functions
/// </summary>
public class BlipScriptDefinition
{
    private readonly MtaServer server;

    public BlipScriptDefinition(MtaServer server)
    {
        this.server = server;
    }

    [ScriptFunctionDefinition("createBlip")]
    public Blip CreateBlip(Vector3 position, BlipIcon icon, byte size = 2, int red = 255, int green = 0, int blue = 0, int alpha = 255, short ordering = 0, ushort visibleDistance = 16383)
    {
        return new Blip(position, icon, visibleDistance)
        {
            Color = Color.FromArgb(alpha, red, green, blue),
            Size = size,
            Ordering = ordering
        }.AssociateWith(this.server);
    }

    // TODO: Attach to element
    [ScriptFunctionDefinition("createBlipAttachedTo")]
    public Blip CreateBlipAttachedTo(Element element, BlipIcon icon, byte size = 2, int red = 255, int green = 0, int blue = 0, int alpha = 255, short ordering = 0, ushort visibleDistance = 16383)
    {
        return new Blip(element.Position, icon, visibleDistance)
        {
            Color = Color.FromArgb(alpha, red, green, blue),
            Size = size,
            Ordering = ordering,
        }.AssociateWith(this.server);
    }

    [ScriptFunctionDefinition("getBlipColor")]
    public Color GetBlipColor(Blip blip) => blip.Color;

    [ScriptFunctionDefinition("getBlipIcon")]
    public BlipIcon GetBlipIcon(Blip blip) => blip.Icon;

    [ScriptFunctionDefinition("getBlipOrdering")]
    public short GetBlipOrdering(Blip blip) => blip.Ordering;

    [ScriptFunctionDefinition("getBlipSize")]
    public byte GetBlipSize(Blip blip) => blip.Size;

    [ScriptFunctionDefinition("getBlipVisibleDistance")]
    public ushort GetBlipVisibleDistance(Blip blip) => blip.VisibleDistance;

    [ScriptFunctionDefinition("setBlipColor")]
    public bool SetBlipColor(Blip blip, int red, int green, int blue, int alpha)
    {
        blip.Color = Color.FromArgb(alpha, red, green, blue);
        return true;
    }

    [ScriptFunctionDefinition("setBlipIcon")]
    public bool SetBlipIcon(Blip blip, BlipIcon icon)
    {
        blip.Icon = icon;
        return true;
    }

    [ScriptFunctionDefinition("setBlipOrdering")]
    public bool SetBlipOrdering(Blip blip, short ordering)
    {
        blip.Ordering = ordering;
        return true;
    }

    [ScriptFunctionDefinition("setBlipSize")]
    public bool SetBlipSize(Blip blip, byte size)
    {
        blip.Size = size;
        return true;
    }

    [ScriptFunctionDefinition("setBlipVisibleDistance")]
    public bool SetBlipVisibleDistance(Blip blip, ushort visibleDistance)
    {
        blip.VisibleDistance = visibleDistance;
        return true;
    }
}
