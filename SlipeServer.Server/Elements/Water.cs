using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A water element
/// Water element represent an arbitrary body of water, defined by a set of vertices.
/// Note that on the client water element vertex positions are rounded to the nearest whole number.
/// </summary>
public class Water : Element
{
    public override ElementType ElementType => ElementType.Water;

    public IEnumerable<Vector3> Vertices { get; set; }
    public bool IsShallow { get; set; } = false;

    public Water(IEnumerable<Vector3> vertices) : base()
    {
        this.Vertices = vertices;
    }

    public new Water AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }
}
