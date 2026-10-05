using Microsoft.Extensions.Logging;
using SlipeServer.Example.Elements;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Console.Logic;

/// <summary>
/// Runtime tests for the sync time context handling when a ped is detached from a vehicle.
///
/// Every relayed warp-into-vehicle and remove-from-vehicle generates a new sync time context and
/// sends it to every player that knows about the ped. The client never generates a context itself,
/// it only adopts the ones the server sends, so a context that is incremented without being
/// broadcast to all players makes the server and those players disagree about the context forever.
/// Those disagreements are what this logic lets you provoke and observe on a live server.
/// </summary>
public class VehicleDetachTimeContextTestLogic
{
    private readonly IMtaServer<CustomPlayer> server;
    private readonly IElementCollection elementCollection;
    private readonly IChatBox chatBox;
    private readonly ICommandService commandService;
    private readonly ILogger<VehicleDetachTimeContextTestLogic> logger;

    private readonly Dictionary<uint, byte> contextSnapshot = new();

    public VehicleDetachTimeContextTestLogic(
        IMtaServer<CustomPlayer> server,
        IElementCollection elementCollection,
        IChatBox chatBox,
        ICommandService commandService,
        ILogger<VehicleDetachTimeContextTestLogic> logger
    )
    {
        this.server = server;
        this.elementCollection = elementCollection;
        this.chatBox = chatBox;
        this.commandService = commandService;
        this.logger = logger;

        this.SetupLogging();
        this.SetupTestCommands();
    }

    private void SetupLogging()
    {
        this.server.ForAny<Vehicle>(vehicle =>
        {
            vehicle.PedEntered += this.HandlePedEntered;
            vehicle.PedLeft += this.HandlePedLeft;
        });
    }

    private void HandlePedEntered(Element sender, VehicleEnteredEventsArgs e)
    {
        this.logger.LogInformation(
            "[tc] {ped} entered vehicle {vehicle} in seat {seat} (warpsIn: {warpsIn}) | {state}",
            e.Ped.Id.Value, e.Vehicle.Id.Value, e.Seat, e.WarpsIn, this.Describe(e.Ped));
    }

    private void HandlePedLeft(Element sender, VehicleLeftEventArgs e)
    {
        this.logger.LogInformation(
            "[tc] {ped} left vehicle {vehicle} from seat {seat} (warpsOut: {warpsOut}) | {state}",
            e.Ped.Id.Value, e.Vehicle.Id.Value, e.Seat, e.WarpsOut, this.Describe(e.Ped));
    }

    private void SetupTestCommands()
    {
        this.commandService.AddCommand("tchelp").Triggered += (source, args) => this.OutputHelp(args.Player);
        this.commandService.AddCommand("tcinfo").Triggered += (source, args) => this.OutputState(args.Player);
        this.commandService.AddCommand("tcdump").Triggered += (source, args) => this.DumpState(args.Player);
        this.commandService.AddCommand("tcsnapshot").Triggered += (source, args) => this.Snapshot(args.Player);
        this.commandService.AddCommand("tccompare").Triggered += (source, args) => this.Compare(args.Player);
        this.commandService.AddCommand("tckill").Triggered += (source, args) => this.KillWhileInVehicle(args.Player);
        this.commandService.AddCommand("tctoss").Triggered += (source, args) => this.TossPedIntoOccupiedSeat(args.Player);
        this.commandService.AddCommand("tcenter").Triggered += (source, args) => this.AbortEntering(args.Player, args.Arguments);
        this.commandService.AddCommand("tcleave").Triggered += (source, args) => this.DetachFromVehicle(args.Player);
        this.commandService.AddCommand("tcjack").Triggered += (source, args) => this.RemoveJackedPed(args.Player);
    }

    private void OutputHelp(Player player)
    {
        this.Output(player, "Vehicle detach sync time context tests:");
        this.Output(player, "  tcinfo      - show your time context, vehicle and the occupants of that vehicle");
        this.Output(player, "  tcdump      - show the time context of every ped and vehicle on the server");
        this.Output(player, "  tcsnapshot  - store the current time contexts of every ped");
        this.Output(player, "  tccompare   - list the time contexts that changed since the snapshot");
        this.Output(player, "  tckill      - kill yourself while seated in a vehicle");
        this.Output(player, "  tctoss      - warp a fresh ped into the seat you are occupying");
        this.Output(player, "  tcenter     - abort an in progress enter of a vehicle, use tcenter <vehicle id> to pick one");
        this.Output(player, "  tcleave     - detach yourself from your vehicle");
        this.Output(player, "  tcjack      - detach the driver of your vehicle as if you jacked him");
        this.Output(player, "");
        this.Output(player, "Entering or leaving a vehicle with warpsIn/warpsOut relayed also generates a time");
        this.Output(player, "context. Run tcsnapshot, perform an action, then tccompare to see which contexts moved.");
        this.Output(player, "A late joining player must not move any context: snapshot, let someone join, then compare.");
    }

    private void OutputState(Player player)
    {
        this.Output(player, this.Describe(player));

        var vehicle = player.Vehicle ?? player.EnteringVehicle;
        if (vehicle == null)
        {
            this.Output(player, "You are neither in nor entering a vehicle.");
            return;
        }

        this.Output(player, this.Describe(vehicle));
        this.OutputOccupants(player, vehicle);
    }

    private void DumpState(Player player)
    {
        this.logger.LogInformation("[tc] --- server time contexts ---");

        foreach (var ped in this.elementCollection.GetByType<Ped>())
            this.logger.LogInformation("[tc] {ped}", this.Describe(ped));

        foreach (var vehicle in this.elementCollection.GetByType<Vehicle>())
            this.logger.LogInformation("[tc] {vehicle}", this.Describe(vehicle));

        this.Output(player, $"Dumped the time context of every ped and vehicle to the server console.");
    }

    private void Snapshot(Player player)
    {
        this.contextSnapshot.Clear();
        foreach (var ped in this.elementCollection.GetByType<Ped>())
            this.contextSnapshot[ped.Id.Value] = ped.TimeContext;

        this.Output(player, $"Stored the time context of {this.contextSnapshot.Count} ped(s).");
    }

    private void Compare(Player player)
    {
        if (this.contextSnapshot.Count == 0)
        {
            this.Output(player, "No snapshot stored yet, run tcsnapshot first.");
            return;
        }

        var changed = 0;
        foreach (var ped in this.elementCollection.GetByType<Ped>())
        {
            if (!this.contextSnapshot.TryGetValue(ped.Id.Value, out var previous))
            {
                this.Output(player, $"{ped.Id.Value} is new, time context {ped.TimeContext}.");
                changed++;
                continue;
            }

            if (previous != ped.TimeContext)
            {
                this.Output(player, $"{ped.Id.Value} time context changed from {previous} to {ped.TimeContext}.");
                changed++;
            }
        }

        this.Output(player, changed == 0
            ? "No time context changed since the snapshot."
            : $"{changed} time context change(s) since the snapshot.");
    }

    private void KillWhileInVehicle(Player player)
    {
        var vehicle = player.Vehicle;
        if (vehicle == null)
        {
            this.Output(player, "You are not in a vehicle.");
            return;
        }

        var contextBefore = player.TimeContext;
        this.Output(player, $"Before: {this.Describe(player)}");
        this.OutputOccupants(player, vehicle);

        player.Kill();

        this.Output(player, $"After: {this.Describe(player)}");
        this.OutputOccupants(player, vehicle);
        this.Output(player, $"Your time context went from {contextBefore} to {player.TimeContext}, expected exactly one increment.");
        this.Output(player, "The vehicle must no longer list you as an occupant, and no remove-from-vehicle RPC may be relayed.");
    }

    private void TossPedIntoOccupiedSeat(Player player)
    {
        var vehicle = player.Vehicle;
        if (vehicle == null)
        {
            this.Output(player, "You are not in a vehicle.");
            return;
        }

        var seat = player.Seat ?? 0;
        this.Output(player, $"Seat {seat} currently holds {this.DescribeSeat(vehicle, seat)}.");

        var ped = new Ped(PedModel.Swat, vehicle.Position).AssociateWith(this.server);
        this.logger.LogInformation("[tc] created ped {ped} for the seat toss test", ped.Id.Value);

        vehicle.AddPassenger(seat, ped, true);

        this.Output(player, $"Seat {seat} now holds {this.DescribeSeat(vehicle, seat)}.");
        this.Output(player, $"A remove-from-vehicle RPC for the previous occupant is only correct if that occupant was still attached to the vehicle.");
        this.Output(player, "Combine this with tckill: kill a seated player first, then toss a ped in his seat.");
    }

    private void AbortEntering(Player player, string[] arguments)
    {
        Vehicle? vehicle = null;
        if (arguments.Length > 0 && uint.TryParse(arguments[0], out var vehicleId))
            vehicle = this.elementCollection.Get(vehicleId) as Vehicle;

        vehicle ??= player.Vehicle ?? player.EnteringVehicle ?? this.FindNearestVehicle(player);
        if (vehicle == null)
        {
            this.Output(player, "No vehicle nearby, pass an id explicitly: tcenter <vehicle id>");
            return;
        }

        if (player.Vehicle != null)
        {
            // Take the player out of the occupant list without relaying anything, so the server ends
            // up in the exact state it is in while the client is playing the enter animation.
            player.Vehicle.RemovePassenger(player, false);
        }

        var contextBefore = player.TimeContext;
        player.Seat ??= 0;
        player.EnteringVehicle = vehicle;
        player.VehicleAction = VehicleAction.Entering;

        this.Output(player, $"Simulating an in progress enter: {this.Describe(player)}");
        this.OutputOccupants(player, vehicle);

        player.RemoveFromVehicle();

        this.Output(player, $"After aborting: {this.Describe(player)}");
        this.Output(player, $"Your time context went from {contextBefore} to {player.TimeContext}, expected no change at all.");
        this.Output(player, "A vehicle-in-out notify-in-abort-return must have been relayed instead of a remove-from-vehicle RPC.");
    }

    private void DetachFromVehicle(Player player)
    {
        var vehicle = player.Vehicle;
        if (vehicle == null)
        {
            this.Output(player, "You are not in a vehicle.");
            return;
        }

        var contextBefore = player.TimeContext;
        this.Output(player, $"Before: {this.Describe(player)}");

        player.RemoveFromVehicle();

        this.Output(player, $"After: {this.Describe(player)}");
        this.OutputOccupants(player, vehicle);
        this.Output(player, $"Your time context went from {contextBefore} to {player.TimeContext}, expected exactly one increment.");
    }

    private void RemoveJackedPed(Player player)
    {
        var vehicle = player.Vehicle;
        if (vehicle == null)
        {
            this.Output(player, "You are not in a vehicle.");
            return;
        }

        var jacked = vehicle.JackingPed ?? vehicle.Driver;
        if (jacked == null || jacked == player)
        {
            this.Output(player, "Your vehicle has no other occupant to treat as jacked.");
            return;
        }

        var contextBefore = jacked.TimeContext;
        this.Output(player, $"Before: {this.Describe(jacked)}");

        vehicle.RemovePassenger(jacked, true);

        this.Output(player, $"After: {this.Describe(jacked)}");
        this.OutputOccupants(player, vehicle);
        this.Output(player, $"The jacked ped's time context went from {contextBefore} to {jacked.TimeContext}, expected exactly one increment.");
        this.Output(player, "A jacked ped is detached the same way as any other passenger, so his removal has to reach every player.");
    }

    private string Describe(Ped ped)
    {
        var kind = ped is Player ? "player" : "ped";
        var vehicle = ped.Vehicle == null ? "none" : ped.Vehicle.Id.Value.ToString();
        var enteringVehicle = ped.EnteringVehicle == null ? "none" : ped.EnteringVehicle.Id.Value.ToString();
        var seat = ped.Seat == null ? "none" : ped.Seat.Value.ToString();

        return $"{kind} {ped.Id.Value}: time context {ped.TimeContext}, seat {seat}, vehicle {vehicle}, " +
            $"entering vehicle {enteringVehicle}, action {ped.VehicleAction}";
    }

    private string Describe(Vehicle vehicle)
        => $"vehicle {vehicle.Id.Value} (model {(VehicleModel)vehicle.Model}): time context {vehicle.TimeContext}, " +
        $"{vehicle.Occupants.Count} occupant(s)";

    private string DescribeSeat(Vehicle vehicle, byte seat)
    {
        var occupant = vehicle.GetOccupantInSeat(seat);
        return occupant == null
            ? "nobody"
            : $"{(occupant is Player ? "player" : "ped")} {occupant.Id.Value} (time context {occupant.TimeContext})";
    }

    private void OutputOccupants(Player player, Vehicle vehicle)
    {
        this.Output(player, this.Describe(vehicle));
        foreach (var (seat, occupant) in vehicle.Occupants.OrderBy(x => x.Key))
            this.Output(player, $"  seat {seat}: {this.Describe(occupant)}");
    }

    private Vehicle? FindNearestVehicle(Player player) => this.elementCollection
        .GetWithinRange<Vehicle>(player.Position, 25)
        .OrderBy(x => Vector3.Distance(player.Position, x.Position))
        .FirstOrDefault();

    private void Output(Player player, string message)
    {
        this.chatBox.OutputTo(player, message, Color.Yellow);
        this.logger.LogInformation("[tc] {message}", message);
    }
}
