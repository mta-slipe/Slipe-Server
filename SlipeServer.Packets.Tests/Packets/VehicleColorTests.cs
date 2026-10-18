using FluentAssertions;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using System.Drawing;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets;

/// <summary>
/// MTA writes the amount of used vehicle colors as a 2 bit value that is one less than that amount, followed by
/// exactly that amount of colors. Unused colors are not written at all, since the client uses black for those.
/// </summary>
public class VehicleColorTests
{
    private static readonly Color Red = Color.FromArgb(255, 0, 0);
    private static readonly Color Green = Color.FromArgb(0, 255, 0);
    private static readonly Color Blue = Color.FromArgb(0, 0, 255);
    private static readonly Color Yellow = Color.FromArgb(255, 255, 0);

    private static (byte ColorCount, Color[] Colors) ReadColors(PacketReader reader)
    {
        var colorCount = reader.GetByteCapped(2);

        var colors = new Color[colorCount + 1];
        for (var i = 0; i < colors.Length; i++)
            colors[i] = Color.FromArgb(reader.GetByte(), reader.GetByte(), reader.GetByte());

        return (colorCount, colors);
    }

    [Fact]
    public void SetVehicleColorWritesAllFourColorsWhenAllAreUsed()
    {
        var packet = new SetVehicleColorRpcPacket((ElementId)1, [Red, Green, Blue, Yellow]);

        var reader = new PacketReader(packet.Write());
        reader.GetByte().Should().Be((byte)ElementRPCFunction.SET_VEHICLE_COLOR);
        reader.GetElementId().Value.Should().Be(1);

        var (colorCount, colors) = ReadColors(reader);

        colorCount.Should().Be(3);
        colors.Should().Equal(Red, Green, Blue, Yellow);
    }

    [Fact]
    public void SetVehicleColorDoesNotWriteUnusedColors()
    {
        var packet = new SetVehicleColorRpcPacket((ElementId)1, [Red, Green, null, null]);

        var reader = new PacketReader(packet.Write());
        reader.GetByte().Should().Be((byte)ElementRPCFunction.SET_VEHICLE_COLOR);
        reader.GetElementId();

        var (colorCount, colors) = ReadColors(reader);

        colorCount.Should().Be(1);
        colors.Should().Equal(Red, Green);
    }

    [Fact]
    public void SetVehicleColorWritesSingleColorWhenOnlyOneIsUsed()
    {
        var packet = new SetVehicleColorRpcPacket((ElementId)1, [Red, null, null, null]);

        var reader = new PacketReader(packet.Write());
        reader.GetByte().Should().Be((byte)ElementRPCFunction.SET_VEHICLE_COLOR);
        reader.GetElementId();

        var (colorCount, colors) = ReadColors(reader);

        colorCount.Should().Be(0);
        colors.Should().Equal(Red);
    }

    [Fact]
    public void VehicleSpawnWritesUsedColors()
    {
        var packet = new VehicleSpawnPacket([
            new VehicleSpawnInfo
            {
                ElementId = (ElementId)1,
                Colors = [Red, Green, Blue, null],
            }
        ]);

        var reader = new PacketReader(packet.Write());
        reader.GetElementId().Value.Should().Be(1);
        reader.GetByte(); // time context
        reader.GetBytes(12); // position
        reader.GetBytes(12); // rotation

        var (colorCount, colors) = ReadColors(reader);

        colorCount.Should().Be(2);
        colors.Should().Equal(Red, Green, Blue);
    }

    [Fact]
    public void WriteVehicleColorsWritesSingleBlackColorWhenNoColorsAreUsed()
    {
        var builder = new PacketBuilder();
        builder.WriteVehicleColors([]);

        var (colorCount, colors) = ReadColors(new PacketReader(builder.Build()));

        colorCount.Should().Be(0);
        colors.Should().Equal(Color.FromArgb(0, 0, 0));
    }
}
