using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Ase;

public class AseQuerySerializationTests
{
    private static AseQueryService CreateService(string name, string map = "None")
    {
        var wrapper = new Mock<INetWrapper>(MockBehavior.Strict);
        wrapper.Setup(x => x.GetAsePingStatus()).Returns(new byte[] { 2, 10, 137, 39, 120, 7, 53, 77, 35, 1 });
        wrapper.Setup(x => x.GetAseNetRoute()).Returns(new byte[] { 1, 58, 210, 79 });

        var server = new Mock<IMtaServer>(MockBehavior.Strict);
        server.SetupGet(x => x.GameType).Returns("play");
        server.SetupGet(x => x.MapName).Returns(map);
        server.SetupGet(x => x.HasPassword).Returns(false);
        server.SetupGet(x => x.Uptime).Returns(TimeSpan.FromSeconds(500));
        server.Setup(x => x.GetNetWrapper(22003)).Returns(wrapper.Object);

        var elements = new Mock<IElementCollection>(MockBehavior.Strict);
        elements.Setup(x => x.GetByType<Player>(ElementType.Player)).Returns(Array.Empty<Player>());

        return new AseQueryService(server.Object, new Configuration
        {
            ServerName = name,
            Port = 22003,
            HttpPort = 22005,
            MaxPlayerCount = 32
        }, elements.Object);
    }

    private static byte[] ReadField(BinaryReader reader)
    {
        int length = reader.ReadByte() - 1;
        Assert.InRange(length, 0, 254);
        byte[] bytes = reader.ReadBytes(length);
        Assert.Equal(length, bytes.Length);
        return bytes;
    }

    [Fact]
    public void FullResponsePreservesUtf8FieldBoundaries()
    {
        byte[] packet = CreateService("Łódź").QueryFull(22003);
        using var reader = new BinaryReader(new MemoryStream(packet));
        Assert.Equal("EYE1", Encoding.ASCII.GetString(reader.ReadBytes(4)));
        Assert.Equal("mta", Encoding.UTF8.GetString(ReadField(reader)));
        Assert.Equal("22003", Encoding.UTF8.GetString(ReadField(reader)));
        Assert.Equal("Łódź", Encoding.UTF8.GetString(ReadField(reader)));
        Assert.Equal("play", Encoding.UTF8.GetString(ReadField(reader)));
        Assert.Equal("None", Encoding.UTF8.GetString(ReadField(reader)));
        Assert.Equal("1.6", Encoding.UTF8.GetString(ReadField(reader)));
        Assert.Equal("0", Encoding.UTF8.GetString(ReadField(reader)));
        Assert.Equal("0", Encoding.UTF8.GetString(ReadField(reader)));
        Assert.Equal("32", Encoding.UTF8.GetString(ReadField(reader)));
        Assert.Equal(1, reader.ReadByte());
        Assert.Equal(reader.BaseStream.Length, reader.BaseStream.Position);
    }

    [Fact]
    public void LightResponseUsesNativeBytesAndUptimeBeyondIntTickRange()
    {
        byte[] packet = CreateService("Test").QueryLight(22003);
        using var reader = new BinaryReader(new MemoryStream(packet));
        Assert.Equal("EYE2", Encoding.ASCII.GetString(reader.ReadBytes(4)));
        ReadField(reader);
        Assert.Equal("22003", Encoding.ASCII.GetString(ReadField(reader)));
        ReadField(reader);
        ReadField(reader);
        byte[] metadata = ReadField(reader);
        byte[] expected = Encoding.ASCII.GetBytes("None\0" + "0/32\0" + "9\0" + "0\0")
            .Concat(new byte[] { 2, 10, 137, 39, 120, 7, 53, 77, 35, 1, 0 })
            .Concat(new byte[] { 1, 58, 210, 79, 0 })
            .Concat(Encoding.ASCII.GetBytes("500\0" + "22005"))
            .ToArray();
        Assert.Equal(expected, metadata);
        Assert.Equal("1.6", Encoding.ASCII.GetString(ReadField(reader)));
        Assert.Equal(new byte[] { 0, 0, 0, 32 }, reader.ReadBytes(4));
        Assert.Equal(reader.BaseStream.Length, reader.BaseStream.Position);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CompoundMapTruncationKeepsUtf8AndFollowingFieldsIntact(bool light)
    {
        var service = CreateService("Test", new string('Ł', 300));
        byte[] packet = light ? service.QueryLight(22003) : service.QueryXFireLight();
        using var reader = new BinaryReader(new MemoryStream(packet));
        reader.ReadBytes(4);
        ReadField(reader);
        if (light)
            ReadField(reader);
        ReadField(reader);
        ReadField(reader);
        byte[] compound = ReadField(reader);
        int separator = Array.IndexOf(compound, (byte)0);
        Assert.True(separator > 0);
        string decoded = new UTF8Encoding(false, true).GetString(compound, 0, separator);
        Assert.All(decoded, character => Assert.Equal('Ł', character));
        Assert.Equal("1.6", Encoding.ASCII.GetString(ReadField(reader)));
    }
}
