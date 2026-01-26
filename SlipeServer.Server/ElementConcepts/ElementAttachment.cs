using SlipeServer.Server.Elements;
using System;
using System.Numerics;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents an element that is attached to another element.
/// </summary>
public class ElementAttachment(Element source,
    Element target,
    Vector3? positionOffset = null,
    Vector3? rotationOffset = null)
{
    /// <summary>
    /// The element that is attached to another element
    /// </summary>
    public Element Source { get; } = source;

    /// <summary>
    /// The element that the source is attached to
    /// </summary>
    public Element Target { get; } = target;

    private Vector3 positionOffset = positionOffset ?? Vector3.Zero;
    /// <summary>
    /// The position offset between the source and the target
    /// </summary>
    public Vector3 PositionOffset
    {
        get => this.positionOffset;
        set
        {
            this.positionOffset = value;
            this.PositionOffsetChanged?.Invoke(value);
            this.UpdateAttachedElement();
        }
    }

    private Vector3 rotationOffset = rotationOffset ?? Vector3.Zero;
    /// <summary>
    /// The rotation offset between the source and the target
    /// </summary>
    public Vector3 RotationOffset
    {
        get => this.rotationOffset;
        set
        {
            this.rotationOffset = value;
            this.RotationOffsetChanged?.Invoke(value);
            this.UpdateAttachedElement();
        }
    }

    /// <summary>
    /// Updates the position (and rotation) of the source element based on the position (and rotation) of the target element
    /// </summary>
    public void UpdateAttachedElement()
    {
        this.Source.RunAsSync(() =>
        {
            this.Source.Position = this.Target.Position +
                this.Target.Right * this.PositionOffset.X +
                this.Target.Forward * this.positionOffset.Y +
                this.Target.Up * this.positionOffset.Z;

            this.Source.Rotation = this.Target.Rotation + this.rotationOffset;
        }, this.Target.IsSync);
    }

    /// <summary>
    /// Triggered when the position offset changes
    /// </summary>
    public event Action<Vector3>? PositionOffsetChanged;

    /// <summary>
    /// Triggered when the rotation offset changes
    /// </summary>
    public event Action<Vector3>? RotationOffsetChanged;

    public static implicit operator SlipeServer.Packets.Definitions.Entities.Structs.ElementAttachment?(ElementAttachment? attachment)
    {
        if (attachment == null)
            return null;

        return new Packets.Definitions.Entities.Structs.ElementAttachment()
        {
            ElementId = attachment.Target.Id,
            AttachmentPosition = attachment.PositionOffset,
            AttachmentRotation = attachment.rotationOffset
        };
    }
}
