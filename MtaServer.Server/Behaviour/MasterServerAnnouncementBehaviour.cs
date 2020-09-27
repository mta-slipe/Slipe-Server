using Microsoft.Extensions.Logging;
using MtaServer.Server.AllSeeingEye;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace MtaServer.Server.Behaviour
{
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
            timer.Elapsed += OnTimerElapsed;

            _ = this.AnnounceToMasterServer();
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await AnnounceToMasterServer();
        }

        private async Task AnnounceToMasterServer()
        {
            try
            {
                byte[] data = this.aseQueryService.QueryLight();
                string version = "1.5.7-1.0";
                string extra = "0_0_0_0_0";

                string url = $"{masterServerUrl}?g={configuration.Port}&a={configuration.Port + 123}&h={configuration.HttpPort}&v={version}&x={extra}&ip=0.0.0.0";
                var response = await this.httpClient.PostAsync(url, new ByteArrayContent(data));

                var keyValuePairCollection = HttpUtility.ParseQueryString(await response.Content.ReadAsStringAsync());
                if (int.TryParse(keyValuePairCollection["interval"], out int interval))
                {
                    this.timer.Interval = interval * 1000;
                }

                this.logger.LogInformation($"Master server list announcement result: {keyValuePairCollection["ok_message"]}");
            } catch (HttpRequestException e)
            {
                this.logger.LogError($"Failed to announce to master server list: {e.Message}");
            }
        }
    }
}
