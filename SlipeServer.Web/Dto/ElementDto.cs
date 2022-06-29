using System.Numerics;

namespace SlipeServer.Web.Dto;

public class ElementDto
{
    public int Id { get; set; }
    public Vector3Dto Position { get; set; }
    public Vector3Dto Rotation { get; set; }
    public Vector3Dto Forwards { get; set; }
    public Vector3Dto Right { get; set; }
    public Vector3Dto Up { get; set; }
}
