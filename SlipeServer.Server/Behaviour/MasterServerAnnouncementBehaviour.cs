using Microsoft.Extensions.Logging;
using SlipeServer.Server.AllSeeingEye;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for announcing the server to the master server list
/// </summary>
public class MasterServerAnnouncementBehaviour
{
    private readonly HttpClient httpClient;
    private readonly Configuration configuration;
    private readonly ILogger logger;
    private readonly string masterServerUrl;
    private readonly IAseQueryService aseQueryService;
    private readonly Timer timer;

    public MasterServerAnnouncementBehaviour(HttpClient httpClient, Configuration configuration, ILogger logger, string masterServerUrl, IAseQueryService aseQueryService)
    {
        this.httpClient = httpClient;
        this.configuration = configuration;
        this.logger = logger;
        this.masterServerUrl = masterServerUrl;
        this.aseQueryService = aseQueryService;

        this.timer = new Timer(720_000)
        {
            AutoReset = true,
            Enabled = true
        };
        this.timer.Elapsed += OnTimerElapsed;

        _ = this.AnnounceToMasterServer();
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            await AnnounceToMasterServer();
        }
        catch(Exception ex)
        {
            this.logger.LogError("Failed to announce to master server list: {message}", ex.Message);
        }
    }

    private async Task AnnounceToMasterServer()
    {
        byte[] data = this.aseQueryService.QueryLight(this.configuration.Port);
        string version = "1.7.0-1.0";
        string extra = "0_0_0_0_0";

        string url = $"{this.masterServerUrl}?g={this.configuration.Port}&a={this.configuration.Port + 123}&h={this.configuration.HttpPort}&v={version}&x={extra}&ip={this.configuration.MasterServerHost}";
        var response = await this.httpClient.PostAsync(url, new ByteArrayContent(data));

        var keyValuePairCollection = HttpUtility.ParseQueryString(await response.Content.ReadAsStringAsync());
        if (int.TryParse(keyValuePairCollection["interval"], out int interval))
        {
            this.timer.Interval = interval * 1000;
        }

        this.logger.LogTrace("Master server list announcement result: {result}", keyValuePairCollection["ok_message"]);
    }
}
