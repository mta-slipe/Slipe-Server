using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling.QueueHandlers;

public class PedSyncPacketHandler : IPacketHandler<PedSyncPacket>
{
    private readonly IElementCollection elementRepository;
    private readonly ISyncHandlerMiddleware<PedSyncPacket?> middleware;

    public PacketId PacketId => PacketId.PACKET_ID_PED_SYNC;

    public PedSyncPacketHandler(IElementCollection elementRepository, ISyncHandlerMiddleware<PedSyncPacket?> middleware)
    {
        this.elementRepository = elementRepository;
        this.middleware = middleware;
    }

    public void HandlePacket(IClient client, PedSyncPacket packet)
    {
        List<PedSyncData> pedsToSync = new();

        foreach (var syncData in packet.Syncs)
        {
            Ped pedElement = (Ped)this.elementRepository.Get(syncData.SourceElementId)!;

            if (pedElement != null)
            {
                if (pedElement.Syncer?.Client == client && pedElement.CanUpdateSync(syncData.TimeSyncContext))
                {
                    pedElement.RunAsSync(() =>
                    {
                        if (syncData.Position != null)
                            pedElement.Position = syncData.Position.Value;

                        if (syncData.Rotation != null)
                            pedElement.PedRotation = syncData.Rotation.Value * (180 / MathF.PI);

                        if (syncData.Velocity != null)
                            pedElement.Velocity = syncData.Velocity.Value;

                        if (syncData.Health != null)
                            pedElement.Health = syncData.Health.Value;

                        if (syncData.Armor != null)
                            pedElement.Armor = syncData.Armor.Value;

                        if (syncData.IsOnFire != null)
                            pedElement.IsOnFire = syncData.IsOnFire.Value;

                        if (syncData.IsInWater != null)
                            pedElement.IsInWater = syncData.IsInWater.Value;
                    });

                    pedsToSync.Add(syncData);
                }
            }
        }

        var players = this.middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.Syncs = pedsToSync;
        packet.SendTo(players);
    }
}
