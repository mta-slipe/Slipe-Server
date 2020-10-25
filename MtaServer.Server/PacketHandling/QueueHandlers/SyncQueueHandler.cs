using MtaServer.Packets.Definitions.Sync;
using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using MtaServer.Server.Repositories;
using System;
using System.Diagnostics;
using System.Linq;

namespace MtaServer.Server.PacketHandling.QueueHandlers
{
    public class SyncQueueHandler : WorkerBasedQueueHandler
    {
        private readonly IElementRepository elementRepository;

        public SyncQueueHandler(IElementRepository elementRepository, int sleepInterval, int workerCount): base(sleepInterval, workerCount)
        {
            this.elementRepository = elementRepository;
        }

        protected override void HandlePacket(PacketQueueEntry queueEntry)
        {
            try
            { 
                switch (queueEntry.PacketId)
                {
                    case PacketId.PACKET_ID_CAMERA_SYNC:
                        CameraSyncPacket cameraPureSyncPacket = new CameraSyncPacket();
                        cameraPureSyncPacket.Read(queueEntry.Data);
                        HandleCameraSyncPacket(queueEntry.Client, cameraPureSyncPacket);
                        break;
                    case PacketId.PACKET_ID_PLAYER_PURESYNC:
                        PlayerPureSyncPacket playerPureSyncPacket = new PlayerPureSyncPacket();
                        playerPureSyncPacket.Read(queueEntry.Data);
                        HandleClientPureSyncPacket(queueEntry.Client, playerPureSyncPacket);
                        break;
                    case PacketId.PACKET_ID_VEHICLE_PUSH_SYNC:
                        UnoccupiedVehiclePushSyncPacket packet = new UnoccupiedVehiclePushSyncPacket();
                        packet.Read(queueEntry.Data);
                        HandleUnoccupiedVehiclePushSyncPacket(queueEntry.Client, packet);
                        break;
                }
            } catch (Exception e)
            {
                Debug.WriteLine("Handling packet failed");
                Debug.WriteLine(string.Join(", ", queueEntry.Data));
                Debug.WriteLine($"{e.Message}\n{e.StackTrace}");
            }
        }

        private void HandleCameraSyncPacket(Client client, CameraSyncPacket packet)
        {
            //Debug.WriteLine($"client {client.Id} camera sync: isFixed: {packet.IsFixed}, position: {packet.Position}, lookAt: {packet.LookAt}, target: {packet.TargetId}");
        }

        private void HandleClientPureSyncPacket(Client client, PlayerPureSyncPacket packet)
        {
            client.SendPacket(new ReturnSyncPacket(packet.Position));

            packet.PlayerId = client.Player.Id;
            packet.Latency = 0;

            // TODO make smart sync packet relaying
            foreach (var remotePlayer in this.elementRepository.GetByType<Player>(ElementType.Player))
            {
                if (remotePlayer.Client != client)
                {
                    remotePlayer.Client.SendPacket(packet);
                }
            }

            var player = client.Player;
            player.Position = packet.Position;
            player.Velocity = packet.Velocity;
            player.Health = packet.Health;
            player.Armor = packet.Armor;
            player.AimOrigin = packet.AimOrigin;
            player.AimDirection = packet.AimDirection;

            player.ContactElement = this.elementRepository.Get(packet.ContactElementId);

            player.CurrentWeapon = new PlayerWeapon()
            {
                WeaponType = packet.WeaponType,
                Slot = packet.WeaponSlot,
                Ammo = packet.TotalAmmo,
                AmmoInClip = packet.AmmoInClip
            };

            player.IsInWater = packet.SyncFlags.IsInWater;
            player.IsOnGround = packet.SyncFlags.IsOnGround;
            player.HasJetpack = packet.SyncFlags.HasJetpack;
            player.IsDucked = packet.SyncFlags.IsDucked;
            player.WearsGoggles = packet.SyncFlags.WearsGoggles;
            player.HasContact = packet.SyncFlags.HasContact;
            player.IsChoking = packet.SyncFlags.IsChoking;
            player.AkimboTargetUp = packet.SyncFlags.AkimboTargetUp;
            player.IsOnFire = packet.SyncFlags.IsOnFire;
            player.IsSyncingVelocity = packet.SyncFlags.IsSyncingVelocity;
            player.IsStealthAiming = packet.SyncFlags.IsStealthAiming;

            player.CameraPosition = packet.CameraOrientation.CameraPosition;
            player.CameraDirection = packet.CameraOrientation.CameraForward;
            player.CameraRotation = packet.CameraRotation;
        }

        private void HandleUnoccupiedVehiclePushSyncPacket(Client client, UnoccupiedVehiclePushSyncPacket packet)
        {
            var vehicle = (Vehicle?) this.elementRepository.Get(packet.VehicleId);
            if (vehicle != null)
            {
                var timeNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (vehicle.Syncer != client.Player && (timeNow - vehicle.LastPushTimestamp) >= Vehicle.MIN_PUSH_ANTISPAM_RATE_MS)
                {
                    if (vehicle.Occupants.ElementAtOrDefault(0) == null)
                    {
                        // TODO we need to update whole vehicle pool to adjust the vehicles syncers
                        vehicle.Syncer = client.Player;
                        vehicle.LastPushTimestamp = timeNow;
                    }
                }
            } else
            {
                Debug.WriteLine("Incorrect vehicle id in VEHICLE_PUSH_SYNC packet");
            }
        }
    }
}
