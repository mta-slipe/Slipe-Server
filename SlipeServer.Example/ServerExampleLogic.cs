using BepuPhysics;
using SlipeServer.Navigation;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace SlipeServer.Example;

public class ServerExampleLogic
{
    private readonly CommandService commandService;
    private readonly ChatBox chatBox;
    private readonly MtaServer mtaServer;
    private readonly LuaEventService luaEventService;

    public ServerExampleLogic(CommandService commandService, ChatBox chatBox, MtaServer mtaServer, LuaEventService luaEventService)
    {
        this.commandService = commandService;
        this.chatBox = chatBox;
        this.mtaServer = mtaServer;
        this.luaEventService = luaEventService;
        AddCommand("hello", player =>
        {
            this.chatBox.OutputTo(player, "Hello world");
        });
        AddCommand("tpblueberry", player =>
        {
            player.Position = new Vector3(126.70996f, -75.1543f, 1.570331f);
        });

        AddCommand("gp", player =>
        {
            this.chatBox.Output($"{player.Position}");
        });
        AddCommand("gp2", player =>
        {
            var pos = player.Position;
            this.chatBox.Output($"new Vector3({pos.X:0.00}f, {pos.Y:0.00}f, {pos.Z:0.00}f)");
        });

        AddVehiclesCommands();
        AddNavigationExample();

        this.mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        var resource = new NaviagationResource(this.mtaServer);
        this.mtaServer.AddAdditionalResource(resource, []);
        resource.StartFor(player);
    }

    private void AddVehiclesCommands()
    {
        AddCommand("myvehprintdamage", player =>
        {
            var vehicle = player.Vehicle!;

            this.chatBox.OutputTo(player, "List of damaged vehicles parts:");
            this.chatBox.OutputTo(player, "Doors:");
            foreach (var item in Enum.GetValues<Packets.Enums.VehicleDoor>())
            {
                var state = vehicle.GetDoorState(item);
                if(state != Packets.Enums.VehicleDoorState.ShutIntact)
                    this.chatBox.OutputTo(player, $" {item} - {state}");
            }

            this.chatBox.OutputTo(player, "Panels:");
            foreach (var item in Enum.GetValues<Packets.Enums.VehiclePanel>())
            {
                var state = vehicle.GetPanelState(item);
                if(state != Packets.Enums.VehiclePanelState.Undamaged)
                    this.chatBox.OutputTo(player, $" {item} - {state}");
            }

            this.chatBox.OutputTo(player, "Wheels:");
            foreach (var item in Enum.GetValues<Packets.Enums.VehicleWheel>())
            {
                var state = vehicle.GetWheelState(item);
                if(state != Packets.Enums.VehicleWheelState.Inflated)
                    this.chatBox.OutputTo(player, $" {item} - {state}");
            }
        });

        AddCommand("damagemyveh", player =>
        {
            var vehicle = player.Vehicle!;

            vehicle.SetDoorState(Packets.Enums.VehicleDoor.Hood, Packets.Enums.VehicleDoorState.Missing);
            vehicle.SetPanelState(Packets.Enums.VehiclePanel.FrontBumper, Packets.Enums.VehiclePanelState.Damaged3);
            vehicle.SetWheelState(Packets.Enums.VehicleWheel.FrontLeft, Packets.Enums.VehicleWheelState.FallenOff);
            this.chatBox.OutputTo(player, "Vehicle damaged!");
        });

        AddCommand("fixmyveh", player =>
        {
            var vehicle = player.Vehicle!;

            vehicle.Fix();
            this.chatBox.OutputTo(player, "Vehicle fixed");
        });
    }

    static class RoadType
    {
        public static int Highway { get; } = 5;
        public static int MainRoad { get; } = 8;
        public static int Suburban { get; } = 10;
        public static int DirtRoad { get; } = 12;
    }

    private void AddNavigationExample()
    {
        var graph = new NavigationGraph();

        var node1 = graph.AddNode(new NavigationNode(new Vector3(133.68f, -75.71f, 1.43f)));
        var node2 = graph.AddNode(new NavigationNode(new Vector3(132.96f, -140.79f, 1.43f)));
        var node3 = graph.AddNode(new NavigationNode(new Vector3(133.14f, -211.64f, 1.43f)));
        var node4 = graph.AddNode(new NavigationNode(new Vector3(181.35f, -211.85f, 1.43f)));
        var node5 = graph.AddNode(new NavigationNode(new Vector3(230.69f, -211.89f, 1.43f)));
        var node6 = graph.AddNode(new NavigationNode(new Vector3(281.25f, -211.75f, 1.43f)));
        var node7 = graph.AddNode(new NavigationNode(new Vector3(328.05f, -210.91f, 1.05f)));
        var node8 = graph.AddNode(new NavigationNode(new Vector3(332.80f, -142.10f, 1.43f)));
        var node9 = graph.AddNode(new NavigationNode(new Vector3(332.19f, -76.84f, 1.43f)));
        var node10 = graph.AddNode(new NavigationNode(new Vector3(284.13f, -71.84f, 1.43f)));
        var node11 = graph.AddNode(new NavigationNode(new Vector3(232.86f, -71.97f, 1.43f)));
        var node12 = graph.AddNode(new NavigationNode(new Vector3(182.68f, -71.97f, 1.43f)));
        var node13 = graph.AddNode(new NavigationNode(new Vector3(179.86f, -143.39f, 1.43f)));
        var node14 = graph.AddNode(new NavigationNode(new Vector3(232.61f, -22.15f, 1.43f)));
        var node15 = graph.AddNode(new NavigationNode(new Vector3(183.64f, -25.31f, 1.43f)));
        var node16 = graph.AddNode(new NavigationNode(new Vector3(282.80f, -141.73f, 1.43f)));
        var node17 = graph.AddNode(new NavigationNode(new Vector3(232.29f, -141.55f, 1.43f)));

        graph.Connect(new NavigationEdge(RoadType.MainRoad), true, true, node1, node2, node3, node4, node5, node6, node7, node8, node9, node10, node11, node12);
        graph.Connect(new NavigationEdge(RoadType.MainRoad), true, node2, node13, node4);
        graph.Connect(new NavigationEdge(RoadType.MainRoad), true, node5, node17, node11);
        graph.Connect(new NavigationEdge(RoadType.MainRoad), true, node11, node14, node15, node12);
        graph.Connect(new NavigationEdge(RoadType.MainRoad), true, node6, node16, node8);
        graph.Connect(new NavigationEdge(RoadType.MainRoad), true, node16, node10);

        AddCommand("navigate", player =>
        {
            var result = graph.FindPath(player.Position, new Vector3(233.53f, -209.70f, 1.43f)).Select(x => LuaValue.ArrayFromVector(x.Position)).ToArray();
            var luaValue = new LuaTable(result);
            this.chatBox.OutputTo(player, $"Found path, nodes count: {result.Length}");
            this.luaEventService.TriggerEventFor(player, "showNavigation", player, luaValue);
        });
        AddCommand("navigate2", player =>
        {
            var result = graph.FindPath(player.Position, new Vector3(232.17f, -71.73f, 1.43f)).Select(x => LuaValue.ArrayFromVector(x.Position)).ToArray();
            var luaValue = new LuaTable(result);
            this.chatBox.OutputTo(player, $"Found path, nodes count: {result.Length}");
            this.luaEventService.TriggerEventFor(player, "showNavigation", player, luaValue);
        });

        AddCommand("navigationgetnodeid", player =>
        {
            var result = graph.GetNearestNode(player.Position);
            if (result != null)
            {
                this.chatBox.OutputTo(player, $"Found node id: {result.Id}");
            } else
            {
                this.chatBox.OutputTo(player, "Unable to find node");
            }
        });
    }

    private void AddCommand(string command, Action<Player> callback)
    {
        this.commandService.AddCommand(command).Triggered += (object? sender, Server.Events.CommandTriggeredEventArgs e) =>
        {
            callback(e.Player);
        };
    }
}
