﻿using FluentAssertions;
using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.QueueHandlers
{
    class DummyQueueHandler : ScalingWorkerBasedQueueHandler
    {
        private readonly int handleTimeout;

        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[]
        {
            PacketId.PACKET_ID_PLAYER_NO_SOCKET
        };

        protected override Dictionary<PacketId, Type> PacketTypes => new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_PLAYER_NO_SOCKET] = typeof(NoSocketPacket)
        };

        public DummyQueueHandler(
            int sleepInterval = 10,
            int minWorkerCount = 1,
            int maxWorkerCount = 10,
            int queueHighThreshold = 20,
            int queueLowThreshold = 3,
            int newWorkerTimeout = 5,
            int handleTimeout = 25
        ) : base(
            sleepInterval: sleepInterval,
            minWorkerCount: minWorkerCount,
            maxWorkerCount: maxWorkerCount,
            queueHighThreshold: queueHighThreshold,
            queueLowThreshold: queueLowThreshold,
            newWorkerTimeout: newWorkerTimeout)
        {
            this.handleTimeout = handleTimeout;
        }

        protected override async Task HandlePacket(Client client, Packet packet)
        {
            await Task.Delay(this.handleTimeout);
        }
    }

    public class ScalingWorkerBasedQueueHandlerTest
    {
        [Fact]
        public void PacketCountOverThresholdSpawnsNewWorker()
        {
            var handler = new DummyQueueHandler(
                minWorkerCount: 1,
                maxWorkerCount: 2,
                newWorkerTimeout: 1,
                handleTimeout: 5
            );

            for (int i = 1; i < 25; i++)
            {
                handler.EnqueuePacket(null, PacketId.PACKET_ID_PLAYER_NO_SOCKET, Array.Empty<byte>());
            }

            var startWorkerCount = handler.WorkerCount;

            handler.CheckWorkerCount();

            var endWorkerCount = handler.WorkerCount;

            startWorkerCount.Should().Be(1);
            endWorkerCount.Should().Be(2);
        }

        [Fact]
        public void PacketCountOverThresholdSpawnsNewWorkerUpToLimit()
        {
            var handler = new DummyQueueHandler(
                minWorkerCount: 1,
                maxWorkerCount: 10,
                newWorkerTimeout: 1,
                queueLowThreshold: 1,
                queueHighThreshold: 2,
                handleTimeout: 5
            );

            for (int i = 1; i < 50; i++)
            {
                handler.EnqueuePacket(null, PacketId.PACKET_ID_PLAYER_NO_SOCKET, Array.Empty<byte>());
            }

            var startWorkerCount = handler.WorkerCount;

            for (int i = 0; i < 10; i++)
            {
                handler.CheckWorkerCount();
            }

            var endWorkerCount = handler.WorkerCount;

            startWorkerCount.Should().Be(1);
            endWorkerCount.Should().Be(10);
        }

        [Fact]
        public async Task PacketCountBelowThresholdDespawnsWorker()
        {
            var handler = new DummyQueueHandler(
                minWorkerCount: 1,
                maxWorkerCount: 2,
                newWorkerTimeout: 1,
                queueLowThreshold: 1,
                queueHighThreshold: 2,
                handleTimeout: 5
            );

            for (int i = 1; i < 5; i++)
            {
                handler.EnqueuePacket(null, PacketId.PACKET_ID_PLAYER_NO_SOCKET, Array.Empty<byte>());
            }

            var startWorkerCount = handler.WorkerCount;

            handler.CheckWorkerCount();

            var betweenWorkerCount = handler.WorkerCount;

            while (handler.QueuedPacketCount > 0)
                await Task.Delay(10);

            handler.CheckWorkerCount();

            var endWorkerCount = handler.WorkerCount;

            startWorkerCount.Should().Be(1);
            betweenWorkerCount.Should().Be(2);
            endWorkerCount.Should().Be(1);
        }
    }
}