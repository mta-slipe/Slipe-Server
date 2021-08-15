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

    public enum EIdentifiersBehaviour
    {
        // Ignore missing/duplicated ids, if something wrong with id it gets generated automatically.
        Ignore,
        // Throw an exception if id is not set, invalid or duplicated.
        Throw,
    }

    public class MapLoaderOptions
    {
        public EIdentifiersBehaviour IdentifiersBehaviour { get; set; }
    }

    public sealed class DefaultMapLoaderOptions : MapLoaderOptions
    {
        public DefaultMapLoaderOptions()
        {
            IdentifiersBehaviour = EIdentifiersBehaviour.Ignore;
        }
    }

    public abstract class MapLoader<T> where T : class
    {
        private readonly XmlNamespaceManager xmlNamespaceManager;
        public MapLoader()
        {
            this.xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
            this.xmlNamespaceManager.AddNamespace("edf", "https://wiki.multitheftauto.com/wiki/Resource:Editor/EDF");
        }

        public abstract void Resolve(Resolver<T> resolver);

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
