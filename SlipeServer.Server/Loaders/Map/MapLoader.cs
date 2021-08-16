using SlipeServer.Server.Loaders.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Loaders.Map.ElementsDefinitions;
using SlipeServer.Server.Loaders.Map.Enums;
using SlipeServer.Server.Loaders.Map.Options;

namespace SlipeServer.Server.Loaders.Map
{
    using Map = Elements.Grouped.Map;

    public abstract class MapLoader<T> where T : class
    {
        private readonly XmlNamespaceManager xmlNamespaceManager;
        public MapLoader()
        {
            this.xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
            this.xmlNamespaceManager.AddNamespace("edf", "https://wiki.multitheftauto.com/wiki/Resource:Editor/EDF");
        }

        public abstract void Resolve(Resolver<T> resolver);

        public Map? LoadMap(string fileName, MtaServer server, MapLoaderOptions? mapLoaderOptions = null)
        {
            using(FileStream file = File.OpenRead(fileName))
            {
                return LoadMap(file, server, mapLoaderOptions);
            }
        }

        public Map? LoadMap(Stream stream, MtaServer server, MapLoaderOptions? mapLoaderOptions = null)
        {
            if (mapLoaderOptions == null)
                mapLoaderOptions = new DefaultMapLoaderOptions();

            stream.Position = 0;

            XmlReaderSettings settings = new XmlReaderSettings { NameTable = xmlNamespaceManager.NameTable };

            XmlParserContext context = new XmlParserContext(null, xmlNamespaceManager, "", XmlSpace.Default);
            XmlReader reader = XmlReader.Create(stream, settings, context);

            XmlSerializer serializer = new XmlSerializer(typeof(T), new XmlRootAttribute("map"));
            T? data = serializer.Deserialize(reader) as T;
            if(data != null)
            {
                Map map = new Map();
                Resolver<T> resolver = new(map, data, mapLoaderOptions);
                Resolve(resolver);
                map.AssociateWith(server);
                return map;
            }
            return null;
        }
    }
}
