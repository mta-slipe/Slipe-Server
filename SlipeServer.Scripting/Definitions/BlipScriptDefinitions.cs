using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class BlipScriptDefinition(MtaServer server)
{
    [ScriptFunctionDefinition("createBlip")]
    public Blip CreateBlip(Vector3 position, BlipIcon icon, byte size = 2, int red = 255, int green = 0, int blue = 0, int alpha = 255, short ordering = 0, ushort visibleDistance = 16383)
    {
        var blip = new Blip(position, icon, visibleDistance)
        {
            Color = Color.FromArgb(alpha, red, green, blue),
            Size = size,
            Ordering = ordering
        }.AssociateWith(server);

        if (ScriptExecutionContext.Current?.Owner != null)
            blip.Parent = ScriptExecutionContext.Current.Owner?.DynamicRoot;

        return blip;
    }

    [ScriptFunctionDefinition("createBlipAttachedTo")]
    public Blip CreateBlipAttachedTo(Element element, BlipIcon icon, byte size = 2, int red = 255, int green = 0, int blue = 0, int alpha = 255, short ordering = 0, ushort visibleDistance = 16383)
    {
        var blip = new Blip(element.Position, icon, visibleDistance)
        {
            Color = Color.FromArgb(alpha, red, green, blue),
            Size = size,
            Ordering = ordering,
        }.AssociateWith(server);

        blip.AttachTo(element);

        if (ScriptExecutionContext.Current?.Owner != null)
            blip.Parent = ScriptExecutionContext.Current.Owner?.DynamicRoot;

        return blip;
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
