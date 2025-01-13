﻿using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Server.Elements;
using System;
using System.Numerics;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class ElementPacketFactory
{
    public static SetElementPositionRpcPacket CreateSetPositionPacket(Element element, Vector3 position, bool isWarp = false)
    {
        return new SetElementPositionRpcPacket(element.Id, element.TimeContext.GetAndIncrement(), position, isWarp);
    }

    public static Packet CreateSetRotationPacket(Element element, Vector3 rotation)
    {
        return element switch
        {
            Vehicle => new SetVehicleRotationRpcPacket(element.Id, element.TimeContext.GetAndIncrement(), rotation),
            Ped => new SetPedRotationRpcPacket(element.Id, element.TimeContext.GetAndIncrement(), rotation.Z / (180 / MathF.PI), true),
            WorldObject => new SetObjectRotationRpcPacket(element.Id, rotation),
            _ => throw new NotImplementedException($"Can not create set rotation packet for {element.GetType()}"),
        };
    }

    public static FixVehicleRpcPacket CreateFixVehiclePacket(Element element)
    {
        return new FixVehicleRpcPacket(element.Id, element.TimeContext.GetAndIncrement());
    }
    
    public static SetElementHealthRpcPacket CreateSetHealthPacket(Element element, float health)
    {
        return new SetElementHealthRpcPacket(element.Id, element.TimeContext.GetAndIncrement(), health);
    }

    public static SetElementAlphaRpcPacket CreateSetAlphaPacket(Element element, byte alpha)
    {
        return new SetElementAlphaRpcPacket(element.Id, alpha);
    }

    public static TakePlayerScreenshotPacket CreateTakePlayerScreenshotPacket(ushort sizeX, ushort sizeY, string tag, byte quality, uint maxBandwith, ushort maxPacketSize, Resources.Resource? resource)
    {
        return new TakePlayerScreenshotPacket(sizeX, sizeY, tag, quality, maxBandwith, maxPacketSize, resource?.NetId ?? 0);
    }

    public static SetElementDimensionRpcPacket CreateSetDimensionPacket(Element element, ushort dimension)
    {
        return new SetElementDimensionRpcPacket(element.Id, dimension);
    }

    public static SetElementInteriorRpcPacket CreateSetInteriorPacket(Element element, byte interior)
    {
        return new SetElementInteriorRpcPacket(element.Id, interior);
    }

    public static SetElementCallPropagationEnabledRpcPacket CreateSetCallPropagationEnabledPacket(Element element, bool enabled)
    {
        return new SetElementCallPropagationEnabledRpcPacket(element.Id, enabled);
    }

    public static SetElementCollisionsEnabledRpcPacket CreateSetCollisionsEnabledPacket(Element element, bool enabled)
    {
        return new SetElementCollisionsEnabledRpcPacket(element.Id, enabled);
    }

    public static SetElementFrozenRpcPacket CreateSetElementFrozen(Element element, bool isFrozen)
    {
        return new SetElementFrozenRpcPacket(element.Id, isFrozen);
    }

    public static AttachElementRpcPacket CreateAttachElementPacket(Element element, Element attachedTo, Vector3 offsetPosition, Vector3 offsetRotation)
    {
        return new AttachElementRpcPacket(element.Id, attachedTo.Id, offsetPosition, offsetRotation);
    }

    public static DetachElementRpcPacket CreateDetachElementPacket(Element element, Vector3 offsetPosition)
    {
        return new DetachElementRpcPacket(element.Id, offsetPosition);
    }

    public static SetElementAttachedOffsetsRpcPacket CreateSetElementAttachedOffsetsPacket(Element element, Vector3 offsetPosition, Vector3 offsetRotation)
    {
        return new SetElementAttachedOffsetsRpcPacket(element.Id, offsetPosition, offsetRotation);
    }
}
