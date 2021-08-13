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

namespace SlipeServer.Server.Loaders.Map
{
    using Map = Elements.Grouped.Map;

    public class MapLoaderOptions
    {
    }

    public abstract class MapLoader<T> where T : class
    {
        private readonly XmlNamespaceManager xmlNamespaceManager;
        public MapLoader()
        {
            this.xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
            this.xmlNamespaceManager.AddNamespace("edf", "https://wiki.multitheftauto.com/wiki/Resource:Editor/EDF");
        }

        public Map LoadMap(Stream stream)
        {
            Map map = new Map();

            stream.Position = 0;

            XmlDocument xmlDocument = new XmlDocument();

            XmlReaderSettings settings = new XmlReaderSettings { NameTable = xmlNamespaceManager.NameTable };

            XmlParserContext context = new XmlParserContext(null, xmlNamespaceManager, "", XmlSpace.Default);
            XmlReader reader = XmlReader.Create(stream, settings, context);

            XmlSerializer serializer = new XmlSerializer(typeof(T), new XmlRootAttribute("map"));
            T foo = serializer.Deserialize(reader) as T;
            return map;
        }
    }
}
