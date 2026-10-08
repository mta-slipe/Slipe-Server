using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Rpc;
using SlipeServer.Packets.Structs;
using SlipeServer.Server;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Events;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.PacketHandling.Handlers.Rpc;
using SlipeServer.Server.Services;
using System.Drawing;
using System.Net;
using System.Numerics;

namespace SlipeServer.Console.Logic;

/// <summary>
/// Runtime harness for the "invisible player" bugs: a player inside a vehicle being invisible to a player that
/// joins afterwards, and a player becoming invisible after being warped to another position.
/// </summary>
/// <remarks>
/// <para>
/// There are two kinds of commands. The scenario commands (<c>warpvis-enter</c>, <c>warpvis-warp</c>,
/// <c>warpvis-warpvehicle</c>, <c>warpvis-exit</c>) set the situation up so it can be looked at with two real
/// clients in game.
/// </para>
/// <para>
/// <c>warpvis-verify</c> instead replays the join handshake through the real packet handler against a recording
/// client, and then inspects the bytes that would have been sent. That way both fixes can be checked from a
/// single client, without needing a second one to log in.
/// </para>
/// </remarks>
public class WarpVisibilityTestLogic
{
    private static readonly Vector3 DefaultWarpTarget = new(1500f, -1400f, 20f);

    private readonly ILogger<WarpVisibilityTestLogic> logger;
    private readonly IMtaServer server;
    private readonly IChatBox chatBox;
    private readonly IElementCollection elementCollection;

    public WarpVisibilityTestLogic(
        ILogger<WarpVisibilityTestLogic> logger,
        IMtaServer server,
        IChatBox chatBox,
        IElementCollection elementCollection,
        ICommandService commandService)
    {
        this.logger = logger;
        this.server = server;
        this.chatBox = chatBox;
        this.elementCollection = elementCollection;

        commandService.AddCommand("warpvis", false).Triggered += (_, e) => this.OutputHelp(e.Player);
        commandService.AddCommand("warpvis-enter", false).Triggered += (_, e) => this.EnterNewVehicle(e.Player);
        commandService.AddCommand("warpvis-exit", false).Triggered += (_, e) => this.ExitVehicle(e.Player);
        commandService.AddCommand("warpvis-warp", false).Triggered += (_, e) => this.WarpPlayer(e.Player, e.Arguments);
        commandService.AddCommand("warpvis-warpvehicle", false).Triggered += (_, e) => this.WarpVehicle(e.Player);
        commandService.AddCommand("warpvis-status", false).Triggered += (_, e) => this.OutputStatus(e.Player);
        commandService.AddCommand("warpvis-verify", false).Triggered += (_, e) => this.Verify(e.Player);
    }

    private void OutputHelp(Player to)
    {
        this.chatBox.OutputTo(to, "warpvis-enter - spawn a vehicle and warp yourself into it, then have another player connect", Color.Yellow);
        this.chatBox.OutputTo(to, "warpvis-warp [x y z] - warp yourself to a position (defaults to a position far away)", Color.Yellow);
        this.chatBox.OutputTo(to, "warpvis-warpvehicle - warp the vehicle you are in, with you still inside it", Color.Yellow);
        this.chatBox.OutputTo(to, "warpvis-exit - get out of your vehicle, which is what used to make the bug go away", Color.Yellow);
        this.chatBox.OutputTo(to, "warpvis-status - print the server-side state of every player", Color.Yellow);
        this.chatBox.OutputTo(to, "warpvis-verify - replay a join and check the packets that would be sent to the joining client", Color.Yellow);
    }

    private void EnterNewVehicle(Player player)
    {
        var position = player.Position + new Vector3(3f, 0f, 0f);
        var vehicle = new Vehicle(411, position);
        vehicle.AssociateWith(this.server);

        player.WarpIntoVehicle(vehicle, 0);

        this.chatBox.OutputTo(player, $"Vehicle {vehicle.Id} created at {Format(position)}, you are on seat {player.Seat}.", Color.LightGreen);
        this.chatBox.OutputTo(player, "Have a second player connect now: they should see you sitting in the vehicle.", Color.LightGreen);
        this.logger.LogInformation(
            "Warp visibility test: {player} (id {playerId}) entered vehicle {vehicleId}, time context {timeContext}",
            player.Name, player.Id, vehicle.Id, player.TimeContext);
    }

    private void ExitVehicle(Player player)
    {
        if (player.Vehicle == null)
        {
            this.chatBox.OutputTo(player, "You are not in a vehicle.", Color.OrangeRed);
            return;
        }

        var vehicleId = player.Vehicle.Id;
        player.RemoveFromVehicle();

        this.chatBox.OutputTo(player, $"You got out of vehicle {vehicleId}. You should be visible to everyone again.", Color.LightGreen);
        this.logger.LogInformation(
            "Warp visibility test: {player} left vehicle {vehicleId}, time context {timeContext}",
            player.Name, vehicleId, player.TimeContext);
    }

    private void WarpPlayer(Player player, string[] arguments)
    {
        if (!TryParsePosition(arguments, out var target))
            target = DefaultWarpTarget;

        if (player.Position == target)
        {
            this.chatBox.OutputTo(player, $"You are already at {Format(target)}, warping to the same position relays nothing.", Color.OrangeRed);
            return;
        }

        player.Position = target;

        this.chatBox.OutputTo(player, $"Warped to {Format(target)}. Other players should still see you.", Color.LightGreen);
        this.logger.LogInformation(
            "Warp visibility test: {player} warped to {position}, time context {timeContext}",
            player.Name, target, player.TimeContext);
    }

    private void WarpVehicle(Player player)
    {
        if (player.Vehicle == null)
        {
            this.chatBox.OutputTo(player, "You are not in a vehicle, use warpvis-enter first.", Color.OrangeRed);
            return;
        }

        var vehicle = player.Vehicle;
        var target = vehicle.Position + new Vector3(150f, 0f, 0f);

        vehicle.Position = target;
        // The server does not move the occupants along with a vehicle, the client does, but warping the player
        // as well keeps the server-side position in step with what the client ends up showing.
        player.Position = target;

        this.chatBox.OutputTo(player, $"Vehicle {vehicle.Id} warped to {Format(target)}, you are still on seat {player.Seat}. Other players should still see you.", Color.LightGreen);
        this.logger.LogInformation(
            "Warp visibility test: vehicle {vehicleId} warped to {position} with {player} inside, time context {timeContext}",
            vehicle.Id, target, player.Name, player.TimeContext);
    }

    private void OutputStatus(Player to)
    {
        foreach (var player in this.server.Players)
        {
            var description = player.Vehicle is { } vehicle
                ? $"in vehicle {vehicle.Id} on seat {player.Seat} at {Format(vehicle.Position)}"
                : $"on foot at {Format(player.Position)}";

            this.chatBox.OutputTo(to, $"{player.Name} (id {player.Id}): {description}, time context {player.TimeContext}", Color.White);
        }
    }

    private void Verify(Player to)
    {
        var report = new List<CheckResult>();

        // The console runs with CustomPlayer as its player type, so anything that goes through the join path has
        // to be one too. Same construction as NametagTestLogic.
        var probe = new CustomPlayer(this.server.GetRequiredService<IExplosionService>(), this.server)
        {
            Name = "warpvis-probe",
            NametagText = "warpvis-probe",
            Model = 9,
        };
        var recorder = new RecordingClient(probe);
        probe.Client = recorder;
        probe.AssociateWith(this.server);

        var vehicle = new Vehicle(411, new Vector3(100f, 100f, 20f));

        try
        {
            var occupants = this.server.Players
                .Where(x => x != probe && x.Vehicle != null)
                .Select(x => (Player: x, TimeContext: x.TimeContext, VehicleId: x.Vehicle!.Id, Seat: x.Seat))
                .ToArray();

            var handler = new RpcPacketHandler(
                this.logger,
                this.server,
                this.server.GetRequiredService<IRootElement>(),
                this.elementCollection,
                this.server.Configuration);

            handler.HandlePacket(recorder, CreateInitialDataStreamPacket());

            report.Add(Check(
                occupants.Length > 0,
                "another player is in a vehicle, so the join path is actually exercised (run warpvis-enter first)",
                "no other player is in a vehicle"));

            // The join code used to warp every occupant into its vehicle while only the joining client was
            // allowed to receive packets. That bumped the occupant's time context, so the sync packets it kept
            // sending no longer matched the context the joining client had been told, and both ends ignored
            // them. The occupant stayed put and invisible while the vehicle it was in kept being synced.
            foreach (var occupant in occupants)
            {
                report.Add(Check(
                    occupant.Player.TimeContext == occupant.TimeContext,
                    $"the time context of {occupant.Player.Name} is unchanged by another player joining",
                    $"{occupant.TimeContext} -> {occupant.Player.TimeContext}"));
            }

            var warpRpcs = DecodeWarpIntoVehicleRpcs(recorder.Packets).ToArray();
            foreach (var occupant in occupants)
            {
                var matching = warpRpcs.Where(x => x.PlayerId == occupant.Player.Id).ToArray();
                report.Add(Check(
                    matching.Length == 1 && matching[0].TimeContext == occupant.Player.TimeContext,
                    $"the joining client is told to warp {occupant.Player.Name} into vehicle {occupant.VehicleId} on seat {occupant.Seat}, using the current time context",
                    matching.Length switch
                    {
                        0 => "no warp into vehicle RPC was sent",
                        _ => $"{matching.Length} warp RPC(s) carrying time context {string.Join("/", matching.Select(x => x.TimeContext))}, expected one carrying {occupant.Player.TimeContext}",
                    }));
            }

            // The player list has to be readable in the exact order the MTA client reads it. Writing one bit too
            // few or too many shifts every following entry, which desynchronises the entire list.
            var playerLists = DecodePlayerLists(recorder.Packets).ToArray();
            report.Add(Check(
                playerLists.Length == 1,
                "exactly one player list is sent when a player joins",
                $"{playerLists.Length} player list(s) received"));

            var expectedListIds = this.server.Players
                .Where(x => x != probe && x.Client.ConnectionState == ClientConnectionState.Joined)
                .Select(x => x.Id.Value)
                .OrderBy(x => x)
                .ToArray();

            foreach (var list in playerLists)
            {
                var readIds = list.Entries.Select(x => x.PlayerId.Value).OrderBy(x => x).ToArray();
                report.Add(Check(
                    readIds.SequenceEqual(expectedListIds),
                    "the player list holds exactly the players that have joined",
                    $"read {readIds.Length} entries ({string.Join(", ", readIds)}), expected {expectedListIds.Length} ({string.Join(", ", expectedListIds)})"));

                report.Add(Check(
                    !list.Overran && list.UnreadBits is >= 0 and < 8,
                    "reading the player list ends inside the packet, on a byte boundary",
                    list.Overran
                        ? "reading the list ran off the end of the packet"
                        : $"{list.UnreadBits} bits left over after reading, expected 0..7"));

                // A list holding a single entry reads back correctly however many bits the writer got wrong,
                // since there is no following entry for it to be shifted against, so only a longer list can
                // actually show that the entries line up.
                report.Add(Check(
                    expectedListIds.Length > 1,
                    "the player list holds more than one entry, so that a shift between entries would show up",
                    $"only {expectedListIds.Length} player has joined, connect another player and run warpvis-verify again"));

                foreach (var occupant in occupants)
                {
                    var entry = list.Entries.FirstOrDefault(x => x.PlayerId == occupant.Player.Id);
                    report.Add(Check(
                        entry != null
                            && entry.IsInVehicle
                            && entry.VehicleId == occupant.VehicleId
                            && entry.Seat == occupant.Seat
                            && entry.TimeContext == occupant.Player.TimeContext,
                        $"the player list tells the joining client that {occupant.Player.Name} is in vehicle {occupant.VehicleId} on seat {occupant.Seat}",
                        entry == null
                            ? "no entry for this player"
                            : $"isInVehicle={entry.IsInVehicle} vehicle={entry.VehicleId} seat={entry.Seat} timeContext={entry.TimeContext}"));
                }
            }

            // A relayed position change has to be a warp. The client only teleports the element, moving it
            // inside the game world and updating the sectors it is registered in, when the warp byte is absent.
            // Without that it only writes the position matrix and stays at its old position as far as the game
            // world is concerned, which is exactly what makes it invisible to other players.
            recorder.Clear();
            var playerWarpTarget = probe.Position + new Vector3(0f, 0f, 5f);
            probe.Position = playerWarpTarget;

            var playerPositionRpcs = DecodeSetElementPositionRpcs(recorder.Packets).ToArray();
            report.Add(Check(
                playerPositionRpcs.Length > 0,
                "changing a player's position relays a set element position RPC",
                "no position RPC was received"));

            foreach (var rpc in playerPositionRpcs)
            {
                report.Add(Check(
                    rpc.UnreadBits is >= 0 and < 8,
                    "the position RPC ends right after the time context, which is what tells the client to teleport",
                    $"{rpc.UnreadBits} bits left over after reading, expected 0..7"));

                report.Add(Check(
                    rpc.Position == playerWarpTarget && rpc.TimeContext == probe.TimeContext,
                    "the position RPC carries the new position and the current time context",
                    $"position {Format(rpc.Position)} and time context {rpc.TimeContext}, expected {Format(playerWarpTarget)} and {probe.TimeContext}"));
            }

            // Warping a vehicle goes through the same relay, and a player inside one is moved along by the client.
            vehicle.AssociateWith(this.server);
            probe.WarpIntoVehicle(vehicle, 0);

            recorder.Clear();
            var vehicleWarpTarget = vehicle.Position + new Vector3(50f, 0f, 0f);
            vehicle.Position = vehicleWarpTarget;

            var vehiclePositionRpcs = DecodeSetElementPositionRpcs(recorder.Packets)
                .Where(x => x.ElementId == vehicle.Id)
                .ToArray();

            var vehicleRpcsAreWarps = vehiclePositionRpcs.Length > 0;
            foreach (var rpc in vehiclePositionRpcs)
                vehicleRpcsAreWarps &= rpc.UnreadBits is >= 0 and < 8 && rpc.Position == vehicleWarpTarget;

            report.Add(Check(
                vehicleRpcsAreWarps,
                "warping a vehicle with a player inside it relays a warp position RPC as well",
                vehiclePositionRpcs.Length == 0
                    ? "no position RPC for the vehicle"
                    : string.Join(", ", vehiclePositionRpcs.Select(x => $"position {Format(x.Position)}, {x.UnreadBits} bits left over"))));
        }
        finally
        {
            // TriggerDisconnected destroys the probe, which also cleans up the blip a CustomPlayer creates for
            // itself, so nothing is left behind in the element collection.
            probe.TriggerDisconnected(QuitReason.Quit);
            vehicle.Destroy();
        }

        OutputReport(to, report);
    }

    private void OutputReport(Player to, IReadOnlyList<CheckResult> report)
    {
        var passed = report.Count(x => x.Passed);

        this.chatBox.OutputTo(
            to,
            $"warp visibility verification: {passed}/{report.Count} checks passed",
            passed == report.Count ? Color.LightGreen : Color.OrangeRed);

        foreach (var result in report)
        {
            var line = $"{(result.Passed ? "PASS" : "FAIL")} {result.Description}{(result.Details == null ? "" : $" -- {result.Details}")}";
            this.chatBox.OutputTo(to, line, result.Passed ? Color.LightGreen : Color.OrangeRed);
            this.logger.LogInformation("Warp visibility check: {line}", line);
        }

        this.logger.LogInformation(
            "Warp visibility verification: {passed}/{total} checks passed{details}",
            passed,
            report.Count,
            report.Any(x => !x.Passed)
                ? $", failures: {string.Join(" | ", report.Where(x => !x.Passed).Select(x => $"{x.Description} ({x.Details})"))}"
                : "");
    }

    private static bool TryParsePosition(string[] arguments, out Vector3 position)
    {
        position = DefaultWarpTarget;

        if (arguments.Length < 3)
            return false;

        if (!float.TryParse(arguments[0], out var x)
            || !float.TryParse(arguments[1], out var y)
            || !float.TryParse(arguments[2], out var z))
            return false;

        position = new Vector3(x, y, z);
        return true;
    }

    private static string Format(Vector3 position) => $"({position.X:0.#}, {position.Y:0.#}, {position.Z:0.#})";

    private static CheckResult Check(bool passed, string description, string details)
        => new(passed, description, passed ? null : details);

    private readonly record struct CheckResult(bool Passed, string Description, string? Details);

    private static RpcPacket CreateInitialDataStreamPacket()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)RpcFunctions.INITIAL_DATA_STREAM);

        var packet = new RpcPacket();
        packet.Read(builder.Build());
        return packet;
    }

    private record RecordedPacket(PacketId PacketId, byte[] Data);

    /// <summary>
    /// A client that records everything the server sends it instead of putting it on the wire, so the exact
    /// bytes of a relay can be inspected without a real MTA client being connected.
    /// </summary>
    private sealed class RecordingClient(Player player) : IClient
    {
        private readonly Lock packetsLock = new();
        private readonly List<RecordedPacket> packets = [];

        public Player Player { get; set; } = player;
        public string? Serial { get; set; } = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        public string? Extra { get; set; }
        public string? Version { get; set; } = "1.6.0-001";
        public IPAddress? IPAddress { get; set; } = IPAddress.Loopback;
        public bool IsConnected { get; set; } = true;
        public ClientConnectionState ConnectionState { get; set; } = ClientConnectionState.Joined;
        public uint Ping { get; set; }

        public IReadOnlyList<RecordedPacket> Packets
        {
            get { lock (this.packetsLock) return [.. this.packets]; }
        }

        public void Clear()
        {
            lock (this.packetsLock)
                this.packets.Clear();
        }

        public void SendPacket(Packet packet) => this.Record(packet.PacketId, packet.Write());

        public void SendPacket(PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.Medium, PacketReliability reliability = PacketReliability.Unreliable)
            => this.Record(packetId, data);

        public void SetVersion(ushort version) { }
        public void ResendModPackets() { }
        public void ResendPlayerACInfo() { }
        public void FetchSerial() { }
        public void FetchIp() { }
        public void SetDisconnected() => this.ConnectionState = ClientConnectionState.Quit;
        public void ResetConnectionState() { }

        private void Record(PacketId packetId, byte[] data)
        {
            lock (this.packetsLock)
                this.packets.Add(new RecordedPacket(packetId, data));
        }
    }

    private record SetElementPositionRpc(ElementId ElementId, Vector3 Position, byte TimeContext, int UnreadBits);

    /// <summary>
    /// Mirrors the read sequence of the MTA:SA client's CElementRPCs::SetElementPosition. The client reads a
    /// warp byte only if one is present, so a packet ending right after the time context is a warp, and a
    /// trailing zero byte means the position change is not a warp.
    /// </summary>
    private static IEnumerable<SetElementPositionRpc> DecodeSetElementPositionRpcs(IEnumerable<RecordedPacket> packets)
    {
        foreach (var packet in packets)
        {
            if (packet.PacketId != PacketId.PACKET_ID_LUA_ELEMENT_RPC || packet.Data.Length < 1)
                continue;
            if (packet.Data[0] != (byte)ElementRpcFunction.SET_ELEMENT_POSITION)
                continue;

            var reader = new PacketReader([.. packet.Data, .. new byte[8]]);
            reader.GetByte();
            var elementId = reader.GetElementId();
            var position = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            var timeContext = reader.GetByte();

            yield return new SetElementPositionRpc(elementId, position, timeContext, (packet.Data.Length * 8) - reader.Counter);
        }
    }

    private record WarpIntoVehicleRpc(ElementId PlayerId, ElementId VehicleId, byte Seat, byte TimeContext);

    /// <summary>
    /// Mirrors the read sequence of the MTA:SA client's CPedRPCs::WarpPedIntoVehicle.
    /// </summary>
    private static IEnumerable<WarpIntoVehicleRpc> DecodeWarpIntoVehicleRpcs(IEnumerable<RecordedPacket> packets)
    {
        foreach (var packet in packets)
        {
            if (packet.PacketId != PacketId.PACKET_ID_LUA_ELEMENT_RPC || packet.Data.Length < 1)
                continue;
            if (packet.Data[0] != (byte)ElementRpcFunction.WARP_PED_INTO_VEHICLE)
                continue;

            var reader = new PacketReader([.. packet.Data, .. new byte[8]]);
            reader.GetByte();
            yield return new WarpIntoVehicleRpc(reader.GetElementId(), reader.GetElementId(), reader.GetByte(), reader.GetByte());
        }
    }

    private record PlayerListEntry(ElementId PlayerId, byte TimeContext, bool IsInVehicle, ElementId VehicleId, byte Seat);

    private record PlayerListResult(List<PlayerListEntry> Entries, int UnreadBits, bool Overran);

    private static IEnumerable<PlayerListResult> DecodePlayerLists(IEnumerable<RecordedPacket> packets)
    {
        foreach (var packet in packets)
        {
            if (packet.PacketId != PacketId.PACKET_ID_PLAYER_LIST)
                continue;

            yield return ReadPlayerList(packet.Data);
        }
    }

    /// <summary>
    /// Mirrors the read sequence of the MTA:SA client's CPacketHandler::Packet_PlayerList. Reading one bit too
    /// many or too few shifts every following entry, which is reported through the amount of unread bits.
    /// </summary>
    private static PlayerListResult ReadPlayerList(byte[] data)
    {
        var logicalSize = data.Length * 8;
        var reader = new PacketReader([.. data, .. new byte[64]]);

        reader.GetBit();

        var entries = new List<PlayerListEntry>();
        var overran = false;
        try
        {
            while (logicalSize - reader.Counter >= 8)
            {
                var entry = ReadPlayerListEntry(reader);
                if (entry != null)
                    entries.Add(entry);
            }
        }
        catch (IndexOutOfRangeException)
        {
            // The reader does not bounds check, so a list whose entries are shifted against each other keeps
            // reading entries until it runs off the end of the buffer instead of failing in a readable way.
            overran = true;
        }

        return new PlayerListResult(entries, logicalSize - reader.Counter, overran);
    }

    private static PlayerListEntry? ReadPlayerListEntry(PacketReader reader)
    {
        var playerId = reader.GetElementId();
        var timeContext = reader.GetByte();

        var nickLength = reader.GetByte();
        if (nickLength > 0)
            reader.GetStringCharacters(nickLength);

        reader.GetUint16();
        reader.GetUint32();

        reader.GetBit();
        var isSpawned = reader.GetBit();
        var isInVehicle = reader.GetBit();
        reader.GetBit();
        reader.GetBit();
        var hasNametagColor = reader.GetBit();
        reader.GetBit();
        reader.GetBit();

        var nametagLength = reader.GetByte();
        if (nametagLength > 0)
            reader.GetStringCharacters(nametagLength);
        if (hasNametagColor)
            reader.GetBytes(3);

        reader.GetByte();
        reader.GetCompressedUint16();
        if (reader.GetBit())
            reader.GetElementId();

        // Unspawned players have no vehicle or position data, so there is nothing to skip over for them.
        if (!isSpawned)
            return null;

        var vehicleId = default(ElementId);
        var seat = (byte)0;
        if (isInVehicle)
        {
            vehicleId = reader.GetElementId();
            seat = reader.GetByteCapped(4);
        }
        else
        {
            reader.GetVector3WithZAsFloat();
            reader.GetFloatFromBits(16, -MathF.PI, MathF.PI);
        }

        reader.GetCompressedUint16();
        reader.GetByte();
        reader.GetCompressedByte();
        reader.GetByte();

        for (var i = 0; i < 16; i++)
            if (reader.GetBit())
                reader.GetWeaponType();

        reader.GetBit();

        return new PlayerListEntry(playerId, timeContext, isInVehicle, vehicleId, seat);
    }
}
