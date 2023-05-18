using SlipeServer.Server.Elements.Events;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A radar area element
/// Radar areas are visual rectangles on the F11 map and ingame radar.
/// </summary>
public class CustomElement : Element
{
    public override ElementType ElementType => ElementType.Custom;

    public CustomElement()
    {
        this.Name = "UnknownCustom";
    }

    public CustomElement(string type)
    {
        this.Name = type;
    }

    public new CustomElement AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

}
