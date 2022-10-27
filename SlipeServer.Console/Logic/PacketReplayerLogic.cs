using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SlipeServer.Console.Logic;

public class PacketReplayerLogic
{
    private readonly MtaServer server;
    private readonly Player replayer;

    public PacketReplayerLogic(MtaServer server, CommandService commandService)
    {
        this.server = server;

        this.server.PlayerJoined += HandlePlayerJoin;
        var client = new FakeClient()
        {
            Serial = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            IPAddress = System.Net.IPAddress.Parse("127.0.0.1"),
            Player = new Player() { 
                Name = "Replayer",
                Position = new System.Numerics.Vector3(0, 0, 3),
                Model = 9,
            }
        };
        client.Player.Client = client;
        this.replayer = client.Player.AssociateWith(this.server);
        this.replayer.AddWeapon(Server.Enums.WeaponId.Deagle, 500);

        commandService.AddCommand("replaypuresync").Triggered += ReplayPackets;
        commandService.AddCommand("replaykeysync").Triggered += ReplayKeySyncPackets;
    }

    private void HandlePlayerJoin(Player obj)
    {

    }

    private async void ReplayPackets(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        var packets = System.IO.Directory.GetFiles("packetlog/puresync")
            .OrderBy(x => x)
            .Select(x => System.IO.File.ReadAllText(x))
            .Select(x => Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerPureSyncPacket>(x))
            .Where(x => x != null);

        foreach (var packet in packets)
        {
            packet!.PlayerId = this.replayer.Id;
            packet.SendTo(e.Player);
            await Task.Delay(50);
            System.Console.WriteLine("Pure Sync packet sent");
        }
    }

    private async void ReplayKeySyncPackets(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        var packets = System.IO.Directory.GetFiles("packetlog/keysync")
            .OrderBy(x => x)
            .Select(x => System.IO.File.ReadAllText(x))
            .Select(x => Newtonsoft.Json.JsonConvert.DeserializeObject<KeySyncPacket>(x))
            .Where(x => x != null);

        foreach (var packet in packets)
        {
            packet!.PlayerId = this.replayer.Id;
            packet.SendTo(e.Player);
            await Task.Delay(50);
            System.Console.WriteLine("Key Sync packet sent");
        }
    }
}
