using SlipeServer.Server;
using System.Xml;
using System.Linq;
using System.Collections;
using System;
using SlipeServer.Server.Enums;

namespace SlipeServer.ConfigurationProviders.Configurations;

public class XmlConfigurationProvider : IConfigurationProvider
{
    public Configuration configuration { get; private set; }
    public Configuration GetConfiguration() => configuration;

    public XmlConfigurationProvider(string fileName)
    {
        configuration = new Configuration();
        XmlDocument xmlConfig = new XmlDocument();
        xmlConfig.Load(fileName);

        ushort result;

        foreach (XmlNode node in xmlConfig.FirstChild.ChildNodes)
        {
            switch (node.Name)
            {
                case "serverName":
                    configuration.ServerName = node.InnerText;
                    break;
                case "host":
                    configuration.Host = node.InnerText;
                    break;
                case "port":
                    if (ushort.TryParse(node.InnerText, out result))
                        configuration.Port = result;

                    break;
                case "maxPlayers":
                    if (ushort.TryParse(node.InnerText, out result))
                        configuration.MaxPlayerCount = result;

                    break;
                case "password":
                    configuration.Password = node.InnerText;
                    break;

                case "httpPort":
                    configuration.HttpPort = ushort.Parse(node.InnerText);
                    break;

                case "httpUrl":
                    configuration.HttpUrl = node.InnerText;
                    break;

                case "httpHost":
                    configuration.HttpHost = node.InnerText;
                    break;

                case "httpConnectionsPerClient":
                    configuration.HttpConnectionsPerClient = int.Parse(node.InnerText);
                    break;

                case "ResourceDirectory":
                    configuration.ResourceDirectory = node.InnerText;
                    break;

                case "ExplosionSyncDistance":
                    configuration.ExplosionSyncDistance = float.Parse(node.InnerText);
                    break;

                case "BulletSyncEnabledWeapons":
                    configuration.BulletSyncEnabledWeapons = node.ChildNodes.Cast<XmlNode>().Select(node => (WeaponId)Enum.Parse(typeof(XmlNode), node.InnerText)).ToArray();
                    break;

                case "VehicleExtrapolationBaseMilliseconds":
                    configuration.VehicleExtrapolationBaseMilliseconds = short.Parse(node.InnerText);
                    break;

                case "VehicleExtrapolationPercentage":
                    configuration.VehicleExtrapolationPercentage = short.Parse(node.InnerText);
                    break;

                case "VehicleExtrapolationMaxMilliseconds":
                    configuration.VehicleExtrapolationMaxMilliseconds = short.Parse(node.InnerText);
                    break;

                case "UseAlternativePulseOrder":
                    configuration.UseAlternativePulseOrder = bool.Parse(node.InnerText);
                    break;

                case "AllowFastSprintFix":
                    configuration.AllowFastSprintFix = bool.Parse(node.InnerText);
                    break;

                case "AllowDriveByAnimationFix":
                    configuration.AllowDriveByAnimationFix = bool.Parse(node.InnerText);
                    break;

                case "AllowShotgunDamageFix":
                    configuration.AllowShotgunDamageFix = bool.Parse(node.InnerText);
                    break;

                case "IsVoiceEnabled":
                    configuration.IsVoiceEnabled = bool.Parse(node.InnerText);
                    break;
            }
        }
    }
}
