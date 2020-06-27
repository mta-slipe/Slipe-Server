using MtaServer.Packets.Definitions.Sync;
using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MtaServer.Server.PacketHandling.QueueHandlers
{
    public class SyncQueueHandler : WorkerBasedQueueHandler
    {
        public SyncQueueHandler(MtaServer server, int sleepInterval, int workerCount): base(server, sleepInterval, workerCount) { }

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
                }
            } catch (Exception e)
            {
                Debug.WriteLine("Handling packet failed");
                Debug.WriteLine(string.Join(", ", queueEntry.Data));
                //Debug.WriteLine($"{e.Message}\n{e.StackTrace}");
            }
        }

        private void HandleCameraSyncPacket(Client client, CameraSyncPacket packet)
        {
            //Debug.WriteLine($"client {client.Id} camera sync: isFixed: {packet.IsFixed}, position: {packet.Position}, lookAt: {packet.LookAt}, target: {packet.TargetId}");
        }

        private void HandleClientPureSyncPacket(Client client, PlayerPureSyncPacket packet)
        {
            client.SendPacket(new ReturnSyncPacket(packet.Position));

            packet.PlayerId = client.Id;
            packet.Latency = 0;
            foreach (var player in this.server.ElementRepository.GetByType<Client>(ElementType.Player))
            {
                if (player != client)
                {
                    player.SendPacket(packet);
                }
            }

            client.Position = packet.Position;

            //Debug.WriteLine($"client {client.Id} pure sync: ");
            //Debug.WriteLine($"\tFlags:"); 

            //Debug.WriteLine($"\t\tIsInWater: {packet.SyncFlags.IsInWater}");
            //Debug.WriteLine($"\t\tIsOnGround: {packet.SyncFlags.IsOnGround}");
            //Debug.WriteLine($"\t\tHasJetpack: {packet.SyncFlags.HasJetpack}");
            //Debug.WriteLine($"\t\tIsDucked: {packet.SyncFlags.IsDucked}");
            //Debug.WriteLine($"\t\tWearsGoggles: {packet.SyncFlags.WearsGoggles}");
            //Debug.WriteLine($"\t\tHasContact: {packet.SyncFlags.HasContact}");
            //Debug.WriteLine($"\t\tIsChoking: {packet.SyncFlags.IsChoking}");
            //Debug.WriteLine($"\t\tAkimboTargetUp: {packet.SyncFlags.AkimboTargetUp}");
            //Debug.WriteLine($"\t\tIsOnFire: {packet.SyncFlags.IsOnFire}");
            //Debug.WriteLine($"\t\tHasAWeapon: {packet.SyncFlags.HasAWeapon}");
            //Debug.WriteLine($"\t\tIsSyncingVelocity: {packet.SyncFlags.IsSyncingVelocity}");
            //Debug.WriteLine($"\t\tIsStealthAiming: {packet.SyncFlags.IsStealthAiming}");

            // Debug.WriteLine($"\tposition: {packet.Position}, rotation: {packet.Rotation}");
            //Debug.WriteLine($"\tvelocity: {packet.Velocity}");
            //Debug.WriteLine($"\thealth: {packet.Health}, armour: {packet.Armour}");
            //Debug.WriteLine($"\tCamera rotation: {packet.CameraRotation}, position: {packet.CameraOrientation.CameraPosition}, forward: {packet.CameraOrientation.CameraForward}");
        }
    }
}
