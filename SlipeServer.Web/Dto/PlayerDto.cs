using System.Numerics;

namespace SlipeServer.Web.Dto;

public class PlayerDto : ElementDto
{
    public string Name { get; set; } = string.Empty;
    public float Health { get; set; }
    public float Armor { get; set; }
}
