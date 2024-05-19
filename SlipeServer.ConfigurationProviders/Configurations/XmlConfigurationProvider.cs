using SlipeServer.Server;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SlipeServer.ConfigurationProviders.Configurations;

public class XmlConfigurationProvider : IConfigurationProvider
{
    public Configuration Configuration { get; private set; }
    public Configuration GetConfiguration() => this.Configuration;

    public XmlConfigurationProvider(string fileName)
    {
        this.Configuration = new Configuration();
        XmlDocument xmlConfig = new XmlDocument();
        xmlConfig.Load(fileName);

        ushort result;

        var startupResources = new List<StartupResource>();

        foreach (XmlNode node in xmlConfig.FirstChild.ChildNodes)
        {
            switch (node.Name)
            {
                case "servername":
                    this.Configuration.ServerName = node.InnerText;
                    break;
                case "serverip":
                    if(node.InnerText == "auto" || node.InnerText == "any")
                    {
                        this.Configuration.Host = "";
                    } else
                    {
                        this.Configuration.Host = node.InnerText;
                    }
                    break;
                case "serverport":
                    if (ushort.TryParse(node.InnerText, out result))
                        this.Configuration.Port = result;

                    break;
                case "maxplayers":
                    if (ushort.TryParse(node.InnerText, out result))
                        this.Configuration.MaxPlayerCount = result;

                    break;
                case "password":
                    this.Configuration.Password = node.InnerText;
                    break;

                case "httpport":
                    this.Configuration.HttpPort = ushort.Parse(node.InnerText);
                    break;

                case "httpurl":
                    this.Configuration.HttpUrl = node.InnerText;
                    break;

                case "httphost":
                    this.Configuration.HttpHost = node.InnerText;
                    break;

                case "httpmaxconnectionsperclient":
                    this.Configuration.HttpConnectionsPerClient = int.Parse(node.InnerText);
                    break;

                case "ResourceDirectory":
                    this.Configuration.ResourceDirectory = node.InnerText;
                    break;

                case "ExplosionSyncDistance":
                    this.Configuration.ExplosionSyncDistance = float.Parse(node.InnerText);
                    break;

                case "BulletSyncEnabledWeapons":
                    this.Configuration.BulletSyncEnabledWeapons = node.ChildNodes.Cast<XmlNode>().Select(node => (WeaponId)Enum.Parse(typeof(XmlNode), node.InnerText)).ToArray();
                    break;

                case "VehicleExtrapolationBaseMilliseconds":
                    this.Configuration.VehicleExtrapolationBaseMilliseconds = short.Parse(node.InnerText);
                    break;

                case "VehicleExtrapolationPercentage":
                    this.Configuration.VehicleExtrapolationPercentage = short.Parse(node.InnerText);
                    break;

                case "VehicleExtrapolationMaxMilliseconds":
                    this.Configuration.VehicleExtrapolationMaxMilliseconds = short.Parse(node.InnerText);
                    break;

                case "UseAlternativePulseOrder":
                    this.Configuration.UseAlternativePulseOrder = bool.Parse(node.InnerText);
                    break;

                case "AllowFastSprintFix":
                    this.Configuration.AllowFastSprintFix = bool.Parse(node.InnerText);
                    break;

                case "AllowDriveByAnimationFix":
                    this.Configuration.AllowDriveByAnimationFix = bool.Parse(node.InnerText);
                    break;

                case "AllowShotgunDamageFix":
                    this.Configuration.AllowShotgunDamageFix = bool.Parse(node.InnerText);
                    break;

                case "IsVoiceEnabled":
                    this.Configuration.IsVoiceEnabled = bool.Parse(node.InnerText);
                    break;

                case "resource":
                    var startupResource = new StartupResource();
                    foreach (XmlAttribute item in node.Attributes)
                    {
                        switch (item.Name)
                        {
                            case "src":
                                startupResource.Name = item.Value;
                                break;
                            case "startup":
                                startupResource.Start = item.Value == "1" || item.Value == "true";
                                break;
                            case "protected":
                                startupResource.Protected = item.Value == "1" || item.Value == "true";
                                break;
                        }
                    }
                    startupResources.Add(startupResource);
                    break;
            }
        }
        
        this.Configuration.StartupResources = startupResources.ToArray();
    }
}
