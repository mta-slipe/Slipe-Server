﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Structures;

public class CameraOrientationStructure : ISyncStructure
{
    private readonly static uint[] bitCountLookup =
    [
            3, 5, 9, 14
    ];

    public Vector3 BasePosition { get; set; }
    public Vector3 CameraPosition { get; set; }
    public Vector3 CameraForward { get; set; }

    public CameraOrientationStructure(Vector3 basePosition)
    {
        this.BasePosition = basePosition;
    }

    public CameraOrientationStructure(
        Vector3 basePosition,
        Vector3 cameraPosition,
        Vector3 cameraForward
    )
    {
        this.BasePosition = basePosition;
        this.CameraPosition = cameraPosition;
        this.CameraForward = cameraForward;
    }

    public void Read(PacketReader reader)
    {
        float zRotation = reader.GetFloatFromBits(8, -MathF.PI, MathF.PI);
        float xRotation = reader.GetFloatFromBits(8, -MathF.PI, MathF.PI);

        float cosxRotation = MathF.Cos(xRotation);
        this.CameraForward = new Vector3(
            cosxRotation * MathF.Sin(zRotation),
            cosxRotation * MathF.Cos(zRotation),
            MathF.Sin(xRotation)
        );

        bool useAbsolutePosition = reader.GetBit();

        byte lookupEntry = reader.GetByteCapped(2, false);
        uint bitCount = bitCountLookup[lookupEntry];
        float range = MathF.Pow(2, bitCount - 1);

        Vector3 cameraPosition = new Vector3(
            reader.GetFloatFromBits(bitCount, -range, range),
            reader.GetFloatFromBits(bitCount, -range, range),
            reader.GetFloatFromBits(bitCount, -range, range)
        );
        this.CameraPosition = useAbsolutePosition ? cameraPosition : (this.BasePosition - cameraPosition);
    }

    public void Write(PacketBuilder builder)
    {

    }
}
