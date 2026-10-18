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
                // `serverName` is Slipe's own name, `servername` is MTA's name. Both are accepted so that
                // MTA's mtaserver.conf can be used as-is.
                case "serverName":
                case "servername":
                    this.Configuration.ServerName = node.InnerText;
                    break;
                case "host":
                    this.Configuration.Host = node.InnerText;
                    break;
                case "serverip":
                    this.Configuration.Host = node.InnerText.ToLowerInvariant() switch
                    {
                        "auto" or "any" or "" => "0.0.0.0",
                        var ip => ip,
                    };
                    break;
                case "port":
                case "serverport":
                    if (ushort.TryParse(node.InnerText, out result))
                        this.Configuration.Port = result;

                    break;
                case "maxPlayers":
                case "maxplayers":
                    if (ushort.TryParse(node.InnerText, out result))
                        this.Configuration.MaxPlayerCount = result;

                    break;
                case "password":
                    this.Configuration.Password = node.InnerText;
                    break;

                case "httpPort":
                case "httpport":
                    this.Configuration.HttpPort = ushort.Parse(node.InnerText);
                    break;

                case "httpUrl":
                case "httpdownloadurl":
                    this.Configuration.HttpUrl = node.InnerText;
                    break;

                case "httpHost":
                    this.Configuration.HttpHost = node.InnerText;
                    break;

                case "httpConnectionsPerClient":
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
                    startupResources.Add(new StartupResource
                    {
                        Name = node.Attributes?["src"]?.Value ?? "",
                        Start = ParseBoolean(node.Attributes?["startup"]?.Value),
                        Protected = ParseBoolean(node.Attributes?["protected"]?.Value),
                    });
                    break;
            }
        }

        this.Configuration.StartupResources = startupResources.ToArray();
    }

    /// <summary>
    /// Parses a boolean the way MTA's mtaserver.conf writes them, accepting `1`, `yes` and `true`.
    /// </summary>
    private static bool ParseBoolean(string? value) =>
        value is not null && (value == "1" || value.Equals("yes", StringComparison.OrdinalIgnoreCase) || value.Equals("true", StringComparison.OrdinalIgnoreCase));
}
