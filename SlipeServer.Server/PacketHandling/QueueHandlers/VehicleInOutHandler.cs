using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SlipeServer.Server.Extensions;
using System.Linq;
using SlipeServer.Packets.Definitions.Player;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SlipeServer.Packets.Definitions.Vehicles;
using System.Numerics;
using SlipeServer.Server.Constants;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class VehicleInOutHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;
        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] { PacketId.PACKET_ID_VEHICLE_INOUT };

        public VehicleInOutHandler(
            ILogger logger,
            MtaServer server, 
            IElementRepository elementRepository, 
            int sleepInterval, 
            int workerCount
        ) : base(sleepInterval, workerCount)
        {
            this.logger = logger;
            this.server = server;
            this.elementRepository = elementRepository;
        }

        protected override void HandlePacket(PacketQueueEntry queueEntry)
        {
            try
            {
                switch (queueEntry.PacketId)
                {
                    case PacketId.PACKET_ID_VEHICLE_INOUT:
                        VehicleInOutPacket inOutPacket = new VehicleInOutPacket();
                        inOutPacket.Read(queueEntry.Data);
                        HandleInOutPacket(queueEntry.Client, inOutPacket);
                        break;
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({queueEntry.PacketId}) failed.\n{e.Message}");
            }
        }

        private void HandleInOutPacket(Client client, VehicleInOutPacket packet)
        {
            var element = this.elementRepository.Get(packet.VehicleId);
            if (element == null)
            {
                this.logger.LogWarning("Attempt to enter non-existant vehicle");
                return;
            }
            this.logger.LogInformation($"Vehicle entry attempt {packet.ActionId}");
            if (element is Vehicle vehicle)
            {
                switch (packet.ActionId)
                {
                    case VehicleInOutAction.RequestIn:
                        HandleRequestIn(client, vehicle, packet);
                        break;
                    case VehicleInOutAction.NotifyIn:
                        HandleNotifyIn(client, vehicle, packet);
                        break;
                    case VehicleInOutAction.NotifyAbortIn:
                        HandleNotifyInAbort(client, vehicle, packet);
                        break;
                    case VehicleInOutAction.RequestOut:
                        HandleRequestOut(client, vehicle, packet);
                        break;
                    case VehicleInOutAction.NotifyOut:
                        HandleNotifyOut(client, vehicle, packet);
                        break;
                    case VehicleInOutAction.NotifyOutAbort:
                        HandleNotifyOutAbort(client, vehicle, packet);
                        break;
                    case VehicleInOutAction.NotifyFellOff:
                        HandleNotifyFellOff(client, vehicle, packet);
                        break;
                    case VehicleInOutAction.NotifyJack:
                        HandleNotifyJack(client, vehicle, packet);
                        break;
                    case VehicleInOutAction.NotifyJackAbort:
                        HandleNotifyJackAbort(client, vehicle, packet);
                        break;
                }
            }
        }

        private void HandleRequestIn(Client client, Vehicle vehicle, VehicleInOutPacket packet)
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
                (client.Player.IsInWater || packet.IsOnWater) && VehicleConstants.WaterEntryVehicles.Contains(vehicle.Model) ||
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
                    client.Player.Vehicle = vehicle;
                    client.Player.VehicleAction = VehicleAction.Entering;



                    // Allow for cancelling of entering
                    /*
                        SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Script);
                        return;
                    */

                    if (warpIn)
                    {
                        // warp ped into vehicle
                    }
                    else
                    {
                        var replyPacket = new VehicleInOutPacket()
                        {
                            PlayerId = client.Player.Id,
                            VehicleId = vehicle.Id,
                            Door = packet.Door,
                            Seat = 0,
                            OutActionId = VehicleInOutActionReturns.RequestInConfirmed,
                        };
                        server.BroadcastPacket(replyPacket);
                    }
                } else
                {

                    // Allow for cancelling of entering
                    /*
                        SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Script);
                        return;
                    */

                    if (vehicle.Driver is Player player)
                    {
                        client.Player.VehicleAction = VehicleAction.Jacking;
                        client.Player.JackingVehicle = vehicle;
                        player.VehicleAction = VehicleAction.Jacked;
                        vehicle.JackingPlayer = client.Player;

                        var replyPacket = new VehicleInOutPacket()
                        {
                            PlayerId = client.Player.Id,
                            VehicleId = vehicle.Id,
                            Door = packet.Door,
                            OutActionId = VehicleInOutActionReturns.RequestJackConfirmed,
                        };
                        server.BroadcastPacket(replyPacket);
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
                client.Player.Vehicle = vehicle;
                client.Player.VehicleAction = VehicleAction.Entering;

                // Allow for cancelling of entering
                /*
                    SendInRequestFailResponse(client, vehicle, VehicleEnterFailReason.Script);
                    return;
                */

                if (warpIn)
                {
                    // warp ped into vehicle
                } else
                {
                    var replyPacket = new VehicleInOutPacket()
                    {
                        PlayerId = client.Player.Id,
                        VehicleId = vehicle.Id,
                        Seat = seat.Value,
                        Door = packet.Door,
                        OutActionId = VehicleInOutActionReturns.RequestInConfirmed,
                    };
                    server.BroadcastPacket(replyPacket);
                }
            }
        }

        private void SendInRequestFailResponse(Client client, Vehicle vehicle, VehicleEnterFailReason failReason)
        {
            var replyPacket = new VehicleInOutPacket()
            {
                PlayerId = client.Player.Id,
                VehicleId = vehicle.Id,
                FailReason = failReason,
                OutActionId = VehicleInOutActionReturns.VehicleAttemptFailed,
                CorrectPosition = (failReason == VehicleEnterFailReason.Distance ? vehicle.Position : Vector3.Zero)
            };
            replyPacket.SendTo(client);
        }

        private void HandleNotifyIn(Client client, Vehicle vehicle, VehicleInOutPacket packet)
        {
            if (client.Player.VehicleAction == VehicleAction.Entering)
            {
                client.Player.VehicleAction = VehicleAction.None;
                if (client.Player.Vehicle != null)
                {
                    vehicle.IsEngineOn = true;
                    vehicle.Occupants[packet.Seat] = client.Player;

                    var replyPacket = new VehicleInOutPacket()
                    {
                        PlayerId = client.Player.Id,
                        VehicleId = vehicle.Id,
                        Seat = packet.Seat,
                        OutActionId = VehicleInOutActionReturns.NotifyInReturn,
                    };
                    server.BroadcastPacket(replyPacket);
                }
            }
        }

        private void HandleNotifyInAbort(Client client, Vehicle vehicle, VehicleInOutPacket packet)
        {
            if (client.Player.VehicleAction == VehicleAction.Entering)
            {
                client.Player.VehicleAction = VehicleAction.None;
                client.Player.Vehicle = null;
                vehicle.RemovePassenger(client.Player);

                var replyPacket = new VehicleInOutPacket()
                {
                    PlayerId = client.Player.Id,
                    VehicleId = vehicle.Id,
                    Seat = packet.Seat,
                    Door = packet.Door,
                    DoorOpenRatio = packet.DoorOpenRatio,
                    OutActionId = VehicleInOutActionReturns.NotifyInAbortReturn,
                };
                server.BroadcastPacket(replyPacket);
            }
        }

        private void HandleRequestOut(Client client, Vehicle vehicle, VehicleInOutPacket packet)
        {
            if (client.Player.VehicleAction != VehicleAction.None)
            {
                var errorReplyPacket = new VehicleInOutPacket()
                {
                    PlayerId = client.Player.Id,
                    VehicleId = vehicle.Id,
                    OutActionId = VehicleInOutActionReturns.VehicleAttemptFailed,
                };
                errorReplyPacket.SendTo(client);
                return;
            }

            // Allow for cancelling of exiting
            /*
                var replyPacket = new VehicleInOutPacket()
                {
                    PlayerId = client.Player.Id,
                    VehicleId = vehicle.Id,
                    OutActionId = VehicleInOutActionReturns.VehicleAttemptFailed,
                };
                replyPacket.SendTo(client);
                return;
            */

            client.Player.VehicleAction = VehicleAction.Exiting;

            var replyPacket = new VehicleInOutPacket()
            {
                PlayerId = client.Player.Id,
                VehicleId = vehicle.Id,
                OutActionId = VehicleInOutActionReturns.RequestOutConfirmed,
                Door = packet.Door
            };
            replyPacket.SendTo(client);
        }

        private void HandleNotifyOut(Client client, Vehicle vehicle, VehicleInOutPacket packet)
        {
            if (client.Player.VehicleAction != VehicleAction.Exiting)
                return;

            client.Player.Vehicle = null;
            client.Player.VehicleAction = VehicleAction.None;

            if (!vehicle.Occupants.ContainsValue(client.Player))
                return;

            vehicle.RemovePassenger(client.Player);

            vehicle.Occupants.Remove(packet.Seat);
            var replyPacket = new VehicleInOutPacket()
            {
                PlayerId = client.Player.Id,
                VehicleId = vehicle.Id,
                OutActionId = VehicleInOutActionReturns.NotifyOutReturn,
                Seat = packet.Seat
            };
            this.server.BroadcastPacket(replyPacket);
        }

        private void HandleNotifyOutAbort(Client client, Vehicle vehicle, VehicleInOutPacket packet)
        {
            if (client.Player.VehicleAction != VehicleAction.Exiting)
                return;

            if (!vehicle.Occupants.ContainsValue(client.Player))
                return;

            client.Player.VehicleAction = VehicleAction.None;


            var replyPacket = new VehicleInOutPacket()
            {
                PlayerId = client.Player.Id,
                VehicleId = vehicle.Id,
                OutActionId = VehicleInOutActionReturns.NotifyOutAbortReturn,
                Seat = packet.Seat
            };
            this.server.BroadcastPacket(replyPacket);
        }

        private void HandleNotifyFellOff(Client client, Vehicle vehicle, VehicleInOutPacket packet)
        {

            if (!vehicle.Occupants.ContainsValue(client.Player))
                return;

            client.Player.VehicleAction = VehicleAction.None;
            client.Player.Vehicle = null;
            vehicle.RemovePassenger(client.Player);


            var replyPacket = new VehicleInOutPacket()
            {
                PlayerId = client.Player.Id,
                VehicleId = vehicle.Id,
                OutActionId = VehicleInOutActionReturns.NotifyFellOffReturn,
                Seat = packet.Seat
            };
            this.server.BroadcastPacket(replyPacket);
        }

        private void HandleNotifyJack(Client client, Vehicle vehicle, VehicleInOutPacket packet)
        {

        }

        private void HandleNotifyJackAbort(Client client, Vehicle vehicle, VehicleInOutPacket packet)
        {

        }
    }
}
