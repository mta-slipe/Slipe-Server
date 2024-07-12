using SlipeServer.Console.Logic;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server;
using SlipeServer.Server.Debugging.PacketRecording;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Services;
using SlipeServer.WebHostBuilderExample;
using System.Diagnostics;
using System.Numerics;
using System.Text;

public class SampleHostedService : IHostedService
{
    private readonly MtaServer mtaServer;
    private readonly ChatBox chatBox;
    private readonly IResourceProvider resourceProvider;

    public SampleHostedService(MtaServer mtaServer, ChatBox chatBox, CommandService commandService, IResourceProvider resourceProvider)
    {
        this.mtaServer = mtaServer;
        this.chatBox = chatBox;
        this.resourceProvider = resourceProvider;
        commandService.AddCommand("startSample").Triggered += HandleStartSample;
        commandService.AddCommand("recordPackets").Triggered += HandleTriggered;
        commandService.AddCommand("vehicleZero").Triggered += HandleVehicleZero;
    }

    private void HandleVehicleZero(object? sender, SlipeServer.Server.Events.CommandTriggeredEventArgs e)
    {
        new Vehicle(404, new Vector3(10, 0, 3))
        {
            Health = 0
        }.AssociateWith(this.mtaServer);
    }

    private async void HandleTriggered(object? sender, SlipeServer.Server.Events.CommandTriggeredEventArgs e)
    {
        var fileName = DateTime.Now.ToString("yyyy-dd-M HH-mm-ss");

        if (!Directory.Exists("packetRecordings"))
            Directory.CreateDirectory("packetRecordings");

        var path = Path.Join("packetRecordings", fileName);
        path = Path.ChangeExtension(path, ".log");
        using var fileStream = File.OpenWrite(path);

        this.chatBox.OutputTo(e.Player, $"Started packets recording.");

        var stopwatch = Stopwatch.StartNew();
        {
            using var recorder = new StreamPacketRecorder(e.Player, this.mtaServer, fileStream,
            [
                PacketId.PACKET_ID_PLAYER_KEYSYNC,
                PacketId.PACKET_ID_PLAYER_PURESYNC,
                PacketId.PACKET_ID_CAMERA_SYNC,
            ]);
            await Task.Delay(10000);

        }
        this.chatBox.OutputTo(e.Player, $"Recorded packets saved to {fileName} file.");
        fileStream.Write(Encoding.UTF8.GetBytes($"Recorded duration: {stopwatch.Elapsed}\n"));

        var line = Encoding.UTF8.GetBytes("End of file\n");
        fileStream.Write(line);
    }

    private void HandleStartSample(object? sender, SlipeServer.Server.Events.CommandTriggeredEventArgs e)
    {
        this.chatBox.OutputTo(e.Player, "Starting sample resource");
        var resource = this.mtaServer.GetAdditionalResource<SampleResource>();
        resource.StartFor(e.Player);
        this.chatBox.OutputTo(e.Player, "Sample resource started");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.mtaServer.InstantiatePersistent<LuaTestLogic>();

        this.mtaServer.PlayerJoined += HandlePlayerJoined;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.mtaServer.PlayerJoined -= HandlePlayerJoined;
        return Task.CompletedTask;
    }

    private void HandlePlayerJoined(Player player)
    {
        var client = player.Client;

        this.chatBox.Output($"{player.Name} ({client.Version}) ({client.Serial}) has joined the server!");

        player.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
        player.Health = 50;
        player.Alpha = 100;
        player.Camera.Target = player;
        player.Camera.Fade(CameraFade.In);
    }
}
