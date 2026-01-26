using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A water element
/// Water element represent an arbitrary body of water, defined by a set of vertices.
/// Note that on the client water element vertex positions are rounded to the nearest whole number.
/// </summary>
public class Water(IEnumerable<Vector3> vertices) : Element()
{
    public override ElementType ElementType => ElementType.Water;

    public IEnumerable<Vector3> Vertices { get; set; } = vertices;
    public bool IsShallow { get; set; } = false;

    public new Water AssociateWith(IMtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }
}
