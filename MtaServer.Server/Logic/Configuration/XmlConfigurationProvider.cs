using System.Xml;

namespace MtaServer.Server.Logic.Configuration
{
    public class XmlConfigurationProvider : ConfigurationProperties, IConfigurationProvider
    {
        public XmlConfigurationProvider(string fileName)
        {
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(fileName);

            ushort result;

            foreach (XmlNode node in xmlConfig.FirstChild.ChildNodes)
            {
                switch(node.Name)
                {
                    case "serverName":
                        serverName = node.InnerText;
                        break;
                    case "host":
                        host = node.InnerText;
                        break;
                    case "port":
                        if (ushort.TryParse(node.InnerText, out result))
                            port = result;

                        break;
                    case "maxPlayers":
                        if (ushort.TryParse(node.InnerText, out result))
                            maxPlayers = result;

                        break;
                    case "password":
                        password = node.InnerText;
                        break;
                }
            }
        }
        public string GetServerName() => serverName;
        public string GetHost() => host;
        public ushort GetPort() => port;
        public ushort GetMaxPlayers() => maxPlayers;
        public string GetPassword() => password;
    }
}
