using FluentAssertions;
using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Structs;
using System;
using System.Drawing;
using System.Numerics;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets;

public class AddEntityPacketTests
{
    private readonly byte[] testPacket =
    [
        249, 155, 2, 8, 255, 255, 192, 48, 120, 116, 168, 202, 230, 232, 64, 238, 194, 232, 202, 228, 0, 9, 245, 254, 0, 0, 0, 1, 0, 129, 251, 254, 0, 0, 0, 1, 0, 129, 245, 254, 6, 0, 0, 1, 0, 129, 251, 254, 6, 0, 0, 1, 0, 128, 156, 2, 1, 255, 255, 192, 48, 88, 118, 168, 202, 230, 232, 64, 222, 196, 212, 202, 198, 232, 0, 0, 0, 0, 1, 217, 254, 0, 0, 128, 128, 0, 0, 0, 0, 0, 0, 65, 1, 131, 255, 255, 80, 0, 2, 2, 3, 67, 206, 129, 1, 127, 255, 224, 24, 60, 57, 84, 101, 115, 116, 32, 98, 108, 105, 112, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 98, 19, 192, 127, 255, 255, 255, 231, 128, 128, 255, 255, 240, 12, 30, 31, 170, 50, 185, 186, 16, 57, 48, 178, 48, 185, 16, 48, 185, 50, 176, 128, 0, 0, 0, 0, 0, 0, 0, 116, 1, 128, 116, 1, 220, 67, 5, 178, 103, 192, 128, 159, 255, 240, 12, 30, 29, 170, 50, 185, 186, 16, 54, 176, 185, 53, 178, 185, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 32, 32, 0, 0, 4, 0, 8, 184, 182, 74, 0, 32, 55, 255, 252, 3, 7, 135, 106, 140, 174, 110, 132, 14, 13, 44, 109, 110, 174, 0, 0, 0, 0, 0, 2, 128, 0, 0, 8, 8, 5, 208, 26, 103, 10, 16, 32, 119, 255, 252, 3, 7, 135, 10, 140, 174, 110, 132, 14, 12, 172, 128, 0, 5, 0, 0, 0, 0, 0, 0, 8, 8, 21, 175, 255, 251, 35, 35, 134, 224, 0, 31, 224, 20, 64, 65, 47, 255, 248, 6, 15, 14, 213, 25, 92, 221, 8, 29, 217, 88, 92, 27, 219, 128, 0, 5, 0, 0, 5, 0, 0, 0, 40, 16, 0, 0, 0, 0, 0, 0, 12, 96, 48, 127, 255, 235, 100, 0, 0, 0, 224, 130, 169, 170, 169, 171, 85, 85, 10, 0, 0, 8, 3, 240, 0, 12, 132, 32, 0, 4, 132, 51, 255, 135, 64, 25, 144, 4, 208, 18, 140, 8, 5, 255, 255, 0, 193, 225, 226, 163, 43, 155, 161, 3, 179, 43, 67, 75, 27, 99, 40, 0, 6, 199, 248, 0, 160, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 6, 86, 131, 143, 248, 0, 0, 0, 7, 248, 0, 0, 0, 0, 0, 0, 0, 1, 0, 160, 20, 2, 128, 80, 10, 1, 0, 41, 166, 36, 168, 34, 144, 16, 16, 16, 2, 55, 201, 184, 109, 128,

    ];

    [Fact]
    public void WritePacket_MatchesExpectedByteArray()
    {
        var packet = new AddEntityPacket();
        packet.AddWater((ElementId)667, (byte)17, null, 0, 0,
            null, true, true, new CustomData(), "Test water",
            0, [
                        new Vector3(-6, 0, 4), new Vector3(-3, 0, 4),
                        new Vector3(-6, 3, 4), new Vector3(-3, 3, 4)
            ], false);
        packet.AddObject(
            (ElementId)668, (byte)3, null, 0, 0,
            null, true, false, new CustomData(), "Test object",
            0, new Vector3(0, -5, 3), Vector3.Zero, 321,
            255, false, null, true, false, true, null, Vector3.One * 3,
            false, 1000f
        );
        packet.AddBlip((ElementId)669, (byte)5, null, 0, 0,
            null, true, true, new CustomData(), "Test blip",
            0, new Vector3(20, 0, 0), 0, 2500, 56, 1, Color.White);
        packet.AddRadarArea((ElementId)670, (byte)7, null, 0, 0,
            null, true, true, new CustomData(), "Test radar area",
            0, new Vector2(0, 0), new Vector2(250, 250), Color.FromArgb(100, Color.DarkGoldenrod), true);
        packet.AddMarker((ElementId)671, (byte)4, null, 0, 0,
            null, true, true, new CustomData(), "Test marker",
            0, new Vector3(5, 0, 2), (byte)2, 2, Color.FromArgb(100, Color.DarkCyan), null);
        packet.AddPickup((ElementId)672, (byte)6, null, 0, 0,
            null, true, true, new CustomData(), "Test pickup",
            0, new Vector3(0, 5, 3), 349, true, (byte)2, null, null, 25, 0);
        packet.AddPed((ElementId)673, (byte)14, null, 0, 0,
            null, true, true, new CustomData(), "Test ped",
            0, new Vector3(10, 0, 3), 181, 45, 100, 50, null, null,
            true, true, true, false, 200, 0, Array.Empty<PedClothing>(), Array.Empty<PedWeapon>(), 0);
        packet.AddWeapon((ElementId)674, (byte)18, null, 0, 0,
            null, true, true, new CustomData(), "Test weapon",
            0, new Vector3(5, 5, 5), Vector3.Zero, 355, 255, false, null,
            true, false, true, null, Vector3.One, false, 100, (byte)0,
            null, null, null, new Vector3(10, 10, 5), true, 10, 1, 100, 200,
            false, false, true, true, true, true, true, true, true, true,
            true, true, true, (byte)0, 1000, 50, (ElementId)666);
        packet.AddVehicle((ElementId)675, (byte)2, null, 0, 0,
            null, true, true, new CustomData(), "Test vehicle",
            0, new Vector3(-10, 5, 3), Vector3.Zero, 602, 1000, 0, [
                        Color.Red, Color.Blue
            ], 0, new VehicleDamage()
            {
                Doors = [0, 0, 0, 0, 0, 0],
                Wheels = [0, 0, 0, 0],
                Panels = [0, 0, 0, 0, 0, 0, 0],
                Lights = [0, 0, 0, 0]
            }, 0, 0, null, null, [
                        0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f
            ], Array.Empty<byte>(), "SLIPE", 0, true, false, false, false, false,
            false, false, false, false, false, false, true, 200, Color.MediumPurple, null, null);

        var result = packet.Write();

        result.Should().Equal(this.testPacket);
    }
}
