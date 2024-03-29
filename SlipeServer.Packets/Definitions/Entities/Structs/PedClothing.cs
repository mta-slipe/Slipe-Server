﻿namespace SlipeServer.Packets.Definitions.Entities.Structs;

public struct PedClothing
{
    public string Texture { get; set; }
    public string Model { get; set; }
    public byte Type { get; set; }

    public PedClothing(string texture, string model, byte type)
    {
        this.Texture = texture;
        this.Model = model;
        this.Type = type;
    }
}
