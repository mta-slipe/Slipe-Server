using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Scripting;
using SlipeServer.Server;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Handlers.Vehicle;
using SlipeServer.Server.Extensions;
using System.Numerics;

namespace SlipeServer.DropInReplacement.PacketHandlers;

public class ScriptingVehicleInOutPacketHandler(
    IElementCollection elementCollection,
    IMtaServer server,
    ILogger logger,
    IScriptEventRuntime eventRuntime
    ) : VehicleInOutPacketHandler(elementCollection, server, logger)
{
    protected override void HandleRequestIn(IClient client, Vehicle vehicle, VehicleInOutPacket packet)
    {
        VehicleEnterFailReason failReason = VehicleEnterFailReason.Invalid;
        if (vehicle.IsTrailer)
        {
            SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Trailer);
            return;
        }
        if (client.Player.VehicleAction != VehicleAction.None)
        {
            SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Action);
            return;
        }
        if (client.Player.Vehicle != null)
        {
            SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.InVehicle);
            return;
        }

        float cutoffDistance = 50f;
        bool warpIn = false;

        if (
            (client.Player.IsInWater || packet.IsOnWater) && VehicleConstants.WaterEntryVehicles.Contains((VehicleModel)vehicle.Model) ||
            vehicle.Model == 464
        )
        {
            cutoffDistance = 10;
            warpIn = true;
        }

        if (vehicle.Driver != null)
            cutoffDistance = 10;

        if (Vector3.Distance(client.Player.Position, vehicle.Position) > cutoffDistance)
        {
            SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Distance);
            return;
        }

        if (failReason != VehicleEnterFailReason.Invalid)
        {
            SendInRequestFailResponse(client, vehicle, failReason);
            return;
        }

        if (packet.Seat == 0)
        {
            if (vehicle.Driver == null)
            {
                if (vehicle.CanEnter != null && !vehicle.CanEnter(client.Player, vehicle, packet.Seat))
                {
                    SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Script);
                    return;
                }

                client.Player.Seat = packet.Seat;
                if (warpIn)
                {
                    vehicle.AddPassenger(packet.Seat, client.Player, true);
                } else
                {
                    // Fire onVehicleStartEnter before confirming so scripts can cancel it
                    vehicle.TriggerPedStartedEntering(client.Player, 0, null, packet.Door);
                    if (eventRuntime.WasEventCancelled())
                    {
                        SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Script);
                        return;
                    }

                    client.Player.EnteringVehicle = vehicle;
                    client.Player.VehicleAction = VehicleAction.Entering;

                    var replyPacket = new VehicleInOutPacket()
                    {
                        PedId = client.Player.Id,
                        VehicleId = vehicle.Id,
                        Door = packet.Door,
                        Seat = 0,
                        OutActionId = VehicleInOutActionReturns.RequestInConfirmed,
                    };
                    Server.BroadcastPacket(replyPacket);
                }
            } else
            {
                if (vehicle.CanEnter?.Invoke(client.Player, vehicle, packet.Seat) == false)
                {
                    SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Script);
                    return;
                }

                if (vehicle.Driver is Ped currentDriver)
                {
                    // Fire onVehicleStartEnter before confirming so scripts can cancel the jack
                    vehicle.TriggerPedStartedEntering(client.Player, 0, currentDriver, packet.Door);
                    if (eventRuntime.WasEventCancelled())
                    {
                        SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Script);
                        return;
                    }

                    client.Player.Seat = packet.Seat;
                    client.Player.VehicleAction = VehicleAction.Jacking;
                    client.Player.JackingVehicle = vehicle;
                    currentDriver.VehicleAction = VehicleAction.Jacked;
                    vehicle.JackingPed = client.Player;

                    var replyPacket = new VehicleInOutPacket()
                    {
                        PedId = client.Player.Id,
                        VehicleId = vehicle.Id,
                        Door = packet.Door,
                        OutActionId = VehicleInOutActionReturns.RequestJackConfirmed,
                    };
                    Server.BroadcastPacket(replyPacket);
                }
            }
        } else
        {
            byte? seat = packet.Seat;
            if (vehicle.GetOccupantInSeat(seat.Value) != null || seat > vehicle.GetMaxPassengers())
            {
                seat = vehicle.GetFreePassengerSeat();
            }
            if (seat == null || seat > 8)
            {
                SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Seat);
                return;
            }
            if (vehicle.CanEnter != null && !vehicle.CanEnter(client.Player, vehicle, packet.Seat))
            {
                SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Script);
                return;
            }

            if (warpIn)
            {
                client.Player.Seat = packet.Seat;
                client.Player.EnteringVehicle = vehicle;
                client.Player.VehicleAction = VehicleAction.Entering;
                vehicle.AddPassenger(seat.Value, client.Player, true);
            } else
            {
                // Fire onVehicleStartEnter before confirming so scripts can cancel it
                vehicle.TriggerPedStartedEntering(client.Player, seat.Value, null, packet.Door);
                if (eventRuntime.WasEventCancelled())
                {
                    SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Script);
                    return;
                }

                client.Player.Seat = seat;
                client.Player.EnteringVehicle = vehicle;
                client.Player.VehicleAction = VehicleAction.Entering;

                var replyPacket = new VehicleInOutPacket()
                {
                    PedId = client.Player.Id,
                    VehicleId = vehicle.Id,
                    Seat = seat.Value,
                    Door = packet.Door,
                    OutActionId = VehicleInOutActionReturns.RequestInConfirmed,
                };
                Server.BroadcastPacket(replyPacket);
            }
        }
    }

    protected override void HandleRequestOut(IClient client, Vehicle vehicle, VehicleInOutPacket packet)
    {
        if (client.Player.VehicleAction != VehicleAction.None)
        {
            var errorReplyPacket = new VehicleInOutPacket()
            {
                PedId = client.Player.Id,
                VehicleId = vehicle.Id,
                OutActionId = VehicleInOutActionReturns.VehicleAttemptFailed,
            };
            errorReplyPacket.SendTo(client);
            return;
        }

        if (vehicle.CanExit != null && !vehicle.CanExit(client.Player, vehicle, packet.Seat))
        {
            var cancelReplyPacket = new VehicleInOutPacket()
            {
                PedId = client.Player.Id,
                VehicleId = vehicle.Id,
                OutActionId = VehicleInOutActionReturns.VehicleAttemptFailed,
            };
            cancelReplyPacket.SendTo(client);
            return;
        }

        // Fire onVehicleStartExit before confirming so scripts can cancel it
        vehicle.TriggerPedStartedExiting(client.Player, client.Player.Seat ?? 0, null, packet.Door);
        if (eventRuntime.WasEventCancelled())
        {
            var cancelReplyPacket = new VehicleInOutPacket()
            {
                PedId = client.Player.Id,
                VehicleId = vehicle.Id,
                OutActionId = VehicleInOutActionReturns.VehicleAttemptFailed,
            };
            cancelReplyPacket.SendTo(client);
            return;
        }

        client.Player.VehicleAction = VehicleAction.Exiting;

        var replyPacket = new VehicleInOutPacket()
        {
            PedId = client.Player.Id,
            VehicleId = vehicle.Id,
            OutActionId = VehicleInOutActionReturns.RequestOutConfirmed,
            Door = packet.Door
        };
        Server.BroadcastPacket(replyPacket);
    }
}
