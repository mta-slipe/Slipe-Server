﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Join;

public sealed class PlayerListPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_LIST;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;


    private readonly PacketBuilder builder;

    public PlayerListPacket(bool showInChat)
    {
        this.builder = new PacketBuilder();

        this.builder.Write(showInChat);
    }

    public void AddPlayer(
        ElementId playerId,
        byte timeContext,
        string nickname,
        ushort bitsreamVersion,
        uint buildNumber,

        bool isDead,
        bool isInVehicle,
        bool hasJetpack,
        bool isNametagShowing,
        bool isHeadless,
        bool isFrozen,

        string nametagText,
        Color? nametagColor,
        byte moveAnimation,

        ushort model,
        ElementId? teamId,

        ElementId? vehicleId,
        byte? seat,

        Vector3? position,
        float? rotation,

        ushort dimension,
        byte fightingStyle,
        byte alpha,
        byte interior,

        byte[] weapons
    )
    {
        this.builder.Write(playerId);
        this.builder.Write(timeContext);
        this.builder.WriteStringWithByteAsLength(nickname);

        this.builder.Write(bitsreamVersion);
        this.builder.Write(buildNumber);

        this.builder.Write(isDead);
        this.builder.Write(true);
        this.builder.Write(isInVehicle);
        this.builder.Write(hasJetpack);
        this.builder.Write(isNametagShowing);
        this.builder.Write(nametagColor != null);
        this.builder.Write(isHeadless);
        this.builder.Write(isFrozen);

        this.builder.WriteStringWithByteAsLength(nametagText);
        if (nametagColor != null)
        {
            this.builder.Write(nametagColor.Value);
        }

        this.builder.Write(moveAnimation);
        this.builder.WriteCompressed(model);
        if (teamId != null)
        {
            this.builder.Write(true);
            this.builder.Write(teamId.Value);
        } else
        {
            this.builder.Write(false);
        }

        if (isInVehicle)
        {
            if (vehicleId == null || seat == null)
            {
                throw new Exception($"Can not write player list packet. {nameof(isInVehicle)} is true, but required data is null");
            }
            this.builder.Write(vehicleId.Value);
            this.builder.WriteCapped(seat.Value, 4);
        } else
        {
            if (position == null || rotation == null)
            {
                throw new Exception($"Can not write player list packet. {nameof(isInVehicle)} is false, but required data is null");
            }
            this.builder.WriteVector3WithZAsFloat(position.Value);
            this.builder.WriteFloatFromBits(rotation.Value, 16, -MathF.PI, MathF.PI, false);
        }

        this.builder.WriteCompressed(dimension);
        this.builder.Write(fightingStyle);
        this.builder.WriteCompressed((byte)(255 - alpha));
        this.builder.Write(interior);

        for (int i = 0; i < 16; ++i)
        {
            if (weapons[i] == 0)
            {
                this.builder.Write(false);
            } else
            {
                this.builder.Write(true);
                this.builder.WriteWeaponType(weapons[i]);
            }
        }
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        return this.builder.Build();
    }
}
