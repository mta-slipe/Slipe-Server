﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public sealed class SetPedAnimationRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public string Block { get; set; }
    public string Animation { get; set; }
    public int Time { get; set; }
    public bool Loops { get; set; }
    public bool UpdatesPosition { get; set; }
    public bool IsInterruptable { get; set; }
    public bool FreezesOnLastFrame { get; set; }
    public int BlendTime { get; set; }
    public bool RestoresTaskOnAnimationEnd { get; set; }

    public SetPedAnimationRpcPacket(
        ElementId elementId,
        string block,
        string animation,
        int time,
        bool loops,
        bool updatesPosition,
        bool isInterruptable,
        bool freezesOnLastFrame,
        int blendTime,
        bool restoresTaskOnAnimationEnd)
    {
        this.ElementId = elementId;
        this.Block = block;
        this.Animation = animation;
        this.Time = time;
        this.Loops = loops;
        this.UpdatesPosition = updatesPosition;
        this.IsInterruptable = isInterruptable;
        this.FreezesOnLastFrame = freezesOnLastFrame;
        this.BlendTime = blendTime;
        this.RestoresTaskOnAnimationEnd = restoresTaskOnAnimationEnd;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_PED_ANIMATION);
        builder.Write(this.ElementId);
        builder.WriteStringWithByteAsLength(this.Block);
        builder.WriteStringWithByteAsLength(this.Animation);
        builder.Write(this.Time);
        builder.Write(this.Loops);
        builder.Write(this.UpdatesPosition);
        builder.Write(this.IsInterruptable);
        builder.Write(this.FreezesOnLastFrame);
        builder.Write(this.BlendTime);
        builder.Write(this.RestoresTaskOnAnimationEnd);

        return builder.Build();
    }
}
