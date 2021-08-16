using FluentAssertions;
using SlipeServer.Packets.Builder;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Grouped;
using SlipeServer.Server.Loaders.Map;
using SlipeServer.Server.Loaders.Map.Enums;
using SlipeServer.Server.Loaders.Map.Exceptions;
using SlipeServer.Server.Loaders.Map.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace SlipeServer.Packets.Tests
{
    public class MapLoaderTests
    {
        DefaultMapLoader mapLoader = new DefaultMapLoader();
        public MapLoaderTests()
        {
            
        }

        [Fact]
        public void EmptyMap()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(@"<map edf:definitions=""editor_main"">");
            streamWriter.WriteLine("</map>");
            streamWriter.Flush();
            Map? map = null;
            try
            {
                map = this.mapLoader.LoadMap(memoryStream);
            }
            catch (Exception exception)
            {
                Assert.True(false, exception.Message);
            }
            if (map != null)
            {
                Assert.Empty(map);
            }
        }

        [Fact]
        public void SingleObject()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(@"<map edf:definitions=""editor_main"">");
            streamWriter.WriteLine(@"    <object id=""Some name"" model=""1337""></object>");
            streamWriter.WriteLine("</map>");
            streamWriter.Flush();
            Map? map = null;
            try
            {
                map = this.mapLoader.LoadMap(memoryStream);
            }
            catch (Exception exception)
            {
                Assert.True(false, exception.Message);
            }
            if (map != null)
            {
                Assert.Single(map);
                WorldObject? worldObject = map["Some name"] as WorldObject;
                Assert.NotNull(worldObject);
                Assert.Equal(worldObject?.Model, (ushort)1337);
                Assert.Equal(worldObject?.Position, Vector3.Zero);
            }
        }


        [Fact]
        public void MapFilewithoutEdfDefinitions()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(@"<map>");
            streamWriter.WriteLine("</map>");
            streamWriter.Flush();
            Map? map = null;
            try
            {
                map = this.mapLoader.LoadMap(memoryStream);
            }
            catch (Exception exception)
            {
                Assert.True(false, exception.Message);
            }
            if (map != null)
            {
                Assert.Empty(map);
            }
        }


        [Fact]
        public void MapFileWithNonStandardRootNode()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(@"<html>");
            streamWriter.WriteLine("</html>");
            streamWriter.Flush();
            try
            {
                Map? map = this.mapLoader.LoadMap(memoryStream);
            }
            catch (Exception)
            {
                Assert.True(true);
                return;
            }
            Assert.True(false);
        }

        [Fact]
        public void InvalidMapFile()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.");
            streamWriter.Flush();
            Assert.Throws<InvalidOperationException>(() => this.mapLoader.LoadMap(memoryStream));
        }

        [Fact]
        public void SampleMap()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(@"<map edf:definitions=""editor_main"">");
            streamWriter.WriteLine(@"    <object id=""Some name1"" model=""1337"" posX=""3"" posY=""4"" posZ=""5""></object>");
            streamWriter.WriteLine(@"    <object id=""Some name2"" model=""1338"" posX=""4"" posY=""5"" posZ=""6""></object>");
            streamWriter.WriteLine(@"    <object id=""Some name3"" model=""1339"" posX=""5"" posY=""6"" posZ=""7""></object>");
            streamWriter.WriteLine("</map>");
            streamWriter.Flush();
            Map? map = null;
            try
            {
                map = this.mapLoader.LoadMap(memoryStream);
            }
            catch (Exception exception)
            {
                Assert.True(false, exception.Message);
            }
            if (map != null)
            {
                Assert.Equal(3, map.Count);

                WorldObject? worldObject1 = map["Some name1"] as WorldObject;
                Assert.Equal(worldObject1?.Model, (ushort)1337);
                Assert.Equal(worldObject1?.Position, new Vector3(3, 4, 5));

                WorldObject? worldObject2 = map["Some name2"] as WorldObject;
                Assert.Equal(worldObject2?.Model, (ushort)1338);
                Assert.Equal(worldObject2?.Position, new Vector3(4, 5, 6));

                WorldObject? worldObject3 = map["Some name3"] as WorldObject;
                Assert.Equal(worldObject3?.Model, (ushort)1339);
                Assert.Equal(worldObject3?.Position, new Vector3(5, 6, 7));
            }
        }

        [Fact]
        public void IgnoreUnsupportedMapElements()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(@"<map edf:definitions=""editor_main"">");
            streamWriter.WriteLine(@"    <foo bar=""123"" />");
            streamWriter.WriteLine(@"    <object model=""1337""></object>");
            streamWriter.WriteLine(@"    <bar foo=""321"" />");
            streamWriter.WriteLine(@"    <object model=""1337""></object>");
            streamWriter.WriteLine(@"    <foo bar=""123"" />");
            streamWriter.WriteLine(@"    <object model=""1337""></object>");
            streamWriter.WriteLine(@"    <bar foo=""321"" />");
            streamWriter.WriteLine(@"    <object model=""1337""></object>");
            streamWriter.WriteLine("</map>");
            streamWriter.Flush();
            Map? map = null;
            try
            {
                map = this.mapLoader.LoadMap(memoryStream);
            }
            catch (Exception exception)
            {
                Assert.True(false, exception.Message);
            }
            Assert.Equal(4, map?.Count);
        }

        [Fact]
        public void TestIfIdsHasBeenGenerated()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(@"<map edf:definitions=""editor_main"">");
            streamWriter.WriteLine(@"    <object model=""1337""></object>");
            streamWriter.WriteLine(@"    <object model=""1337""></object>");
            streamWriter.WriteLine(@"    <object model=""1337""></object>");
            streamWriter.WriteLine(@"    <object model=""1337""></object>");
            streamWriter.WriteLine("</map>");
            streamWriter.Flush();
            Map? map = null;
            try
            {
                map = this.mapLoader.LoadMap(memoryStream);
            }
            catch (Exception exception)
            {
                Assert.True(false, exception.Message);
            }
            if (map != null)
            {
                foreach (var item in map.Keys)
                {
                    Assert.Equal(36, item.Length);
                }
            }
        }

        [Fact]
        public void PreventDuplicatedIds()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(@"<map edf:definitions=""editor_main"">");
            streamWriter.WriteLine(@"    <object id=""Object1"" model=""1337""></object>");
            streamWriter.WriteLine(@"    <object id=""Object2"" model=""1337""></object>");
            streamWriter.WriteLine(@"    <object id=""Object3"" model=""1337""></object>");
            streamWriter.WriteLine(@"    <object id=""Object3"" model=""1337""></object>");
            streamWriter.WriteLine("</map>");
            streamWriter.Flush();
            Map? map = null;
            try
            {
                map = this.mapLoader.LoadMap(memoryStream, null, new MapLoaderOptions
                {
                    IdentifiersBehaviour = IdentifiersBehaviour.Throw,
                });
            }
            catch (DuplicateMapElementIdException exception)
            {
                Assert.Equal("Failed to load map, reason: Duplicated id: 'Object3' for 'object'", exception.Message);
                return;
            }

            Assert.True(false, "Invalid exception has been thrown.");
        }

        [Fact]
        public void InvalidObjectId()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(@"<map edf:definitions=""editor_main"">");
            streamWriter.WriteLine(@"    <object id=""Object1"" model=""1337""></object>");
            streamWriter.WriteLine(@"    <object id=""Object2"" model=""1337""></object>");
            streamWriter.WriteLine(@"    <object id="""" model=""1337""></object>");
            streamWriter.WriteLine(@"    <object id=""Object3"" model=""1337""></object>");
            streamWriter.WriteLine("</map>");
            streamWriter.Flush();
            Map? map = null;
            try
            {
                map = this.mapLoader.LoadMap(memoryStream, null, new MapLoaderOptions
                {
                    IdentifiersBehaviour = IdentifiersBehaviour.Throw,
                });
            }
            catch (InvalidMapElementIdException exception)
            {
                Assert.Equal("Failed to load map, reason: Missing id for 'object'", exception.Message);
                return;
            }

            Assert.True(false, "Invalid exception has been thrown.");
        }
    }
}
