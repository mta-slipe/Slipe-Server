using System.Xml;

namespace MtaServer.ConfigurationProviders.Configurations
{
    public class XmlConfigurationProvider : IConfigurationProvider
    {
        public Configuration configuration { private set; get; }
        public Configuration GetConfiguration() => configuration;

        public XmlConfigurationProvider(string fileName)
        {
            configuration = new Configuration();
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(fileName);

            ushort result;

            foreach (XmlNode node in xmlConfig.FirstChild.ChildNodes)
            {
                switch(node.Name)
                {
                    case "serverName":
                        configuration.serverName = node.InnerText;
                        break;
                    case "host":
                        configuration.host = node.InnerText;
                        break;
                    case "port":
                        if (ushort.TryParse(node.InnerText, out result))
                            configuration.port = result;

                        break;
                    case "maxPlayers":
                        if (ushort.TryParse(node.InnerText, out result))
                            configuration.maxPlayers = result;

                        break;
                    case "password":
                        configuration.password = node.InnerText;
                        break;
                }
            }
        }
    }
}
