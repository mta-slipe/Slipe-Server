using System.Numerics;

namespace SlipeServer.Web.Dto;

public class PlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Forwards { get; set; }
    public Vector3 Right { get; set; }
    public Vector3 Up { get; set; }
    public float Health { get; set; }
    public float Armor { get; set; }
}
