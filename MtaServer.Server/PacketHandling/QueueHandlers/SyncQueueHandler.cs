using MtaServer.Packets.Definitions.Sync;
using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MtaServer.Server.PacketHandling.QueueHandlers
{
    public class SyncQueueHandler : BaseQueueHandler
    {
        private readonly MtaServer server;
        private readonly int sleepInterval;

        public SyncQueueHandler(MtaServer server, int sleepInterval, int workerCount): base()
        {
            this.server = server;
            this.sleepInterval = sleepInterval;

            for (int i = 0; i < workerCount; i++)
            {
                _ = Task.Run(HandlePackets);
            }
        }

        public async void HandlePackets()
        {
            while (true)
            {
                while(this.packetQueue.TryDequeue(out PacketQueueEntry queueEntry))
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
                                HandlePlayerPureSyncPacket(queueEntry.Client, playerPureSyncPacket);
                                break;
                        }
                    } catch (Exception e)
                    {
                        Debug.WriteLine("Handling packet failed");
                        Debug.WriteLine(string.Join(", ", queueEntry.Data));
                        Debug.WriteLine($"{e.Message}\n{e.StackTrace}");
                    }
                }
                await Task.Delay(this.sleepInterval);
            }
        }

        private void HandleCameraSyncPacket(Client client, CameraSyncPacket packet)
        {
            //Debug.WriteLine($"client {client.Id} camera sync: isFixed: {packet.IsFixed}, position: {packet.Position}, lookAt: {packet.LookAt}, target: {packet.TargetId}");
        }

        private void HandlePlayerPureSyncPacket(Client client, PlayerPureSyncPacket packet)
        {
            client.SendPacket(new ReturnSyncPacket(packet.Position));

            packet.PlayerId = client.Player.Id;
            packet.Latency = 0;
            foreach (var remotePlayer in this.server.ElementRepository.GetByType<Player>(ElementType.Player))
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

            player.ContactElement = this.server.ElementRepository.Get(packet.ContactElementId);

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
    }
}
