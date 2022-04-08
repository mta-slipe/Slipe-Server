using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using System;
using System.Numerics;

namespace SlipeServer.Server.Concepts;

public class ElementAttachment
{
    public Element Source { get; }
    public Element Target { get; }

    private Vector3 positionOffset;
    public Vector3 PositionOffset
    {
        get => this.positionOffset;
        set
        {
            this.positionOffset = value;
            this.PositionOffsetChanged?.Invoke(value);
        }
    }

    private Vector3 rotationOffset;
    public Vector3 RotationOffset
    {
        get => this.rotationOffset;
        set
        {
            this.rotationOffset = value;
            this.RotationOffsetChanged?.Invoke(value);
        }
    }

    public ElementAttachment(Element source,
        Element target,
        Vector3? positionOffset = null,
        Vector3? rotationOffset = null)
    {
        this.Source = source;
        this.Target = target;
        this.positionOffset = positionOffset ?? Vector3.Zero;
        this.rotationOffset = rotationOffset ?? Vector3.Zero;
    }

    public void UpdateAttachedElement()
    {
        this.Source.Position = this.Target.Position +
            this.Target.Right * this.PositionOffset.X +
            this.Target.Forward * this.positionOffset.Y +
            this.Target.Up * this.positionOffset.Z;

        this.Source.Rotation = this.Target.Rotation + this.rotationOffset;
    }

    public event Action<Vector3>? PositionOffsetChanged;
    public event Action<Vector3>? RotationOffsetChanged;
}
