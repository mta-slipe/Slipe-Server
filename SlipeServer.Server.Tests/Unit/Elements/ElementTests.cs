using FluentAssertions;
using SlipeServer.Server.Elements;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Elements
{
    public class ElementTests
    {
        [Fact]
        public void GetAndIncrementTimeContext_ReturnsNewTimeContext()
        {
            var element = new Element();

            var context = element.TimeContext;
            var incrementContext = element.GetAndIncrementTimeContext();

            context.Should().NotBe(incrementContext);
        }

        [Fact]
        public void GetAndIncrementTimeContext_WrapsAroundSkippingZero()
        {
            var element = new Element();

            for (int i = 0; i < 256; i++)
            {
                element.GetAndIncrementTimeContext();
            }
            var context = element.TimeContext;

            context.Should().Be(1);
        }

        [Fact]
        public void RunAsSync_TriggersChangeEventsWithSyncTrue()
        {
            var element = new Element();

            bool isEventCalled = false;
            bool eventSyncValue = false;

            element.DimensionChanged += (source, args) =>
            {
                isEventCalled = true;
                eventSyncValue = args.IsSync;
            };

            element.RunAsSync(() =>
            {
                element.Dimension = 1;
            });

            isEventCalled.Should().BeTrue();
            eventSyncValue.Should().BeTrue();
        }

        [Fact]
        public void RunNormally_TriggersChangeEventsWithSyncFalse()
        {
            var element = new Element();

            bool isEventCalled = false;
            bool eventSyncValue = false;

            element.DimensionChanged += (source, args) =>
            {
                isEventCalled = true;
                eventSyncValue = args.IsSync;
            };

            element.Dimension = 1;

            isEventCalled.Should().BeTrue();
            eventSyncValue.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsync_TriggersChangeEventsWithProperSync()
        {
            var element = new Element();

            bool isDimensionEventCalled = false;
            bool dimensionEventSyncValue = false;
            bool isInteriorEventCalled = false;
            bool interiorEventSyncValue = false;

            element.DimensionChanged += (source, args) =>
            {
                isDimensionEventCalled = true;
                dimensionEventSyncValue = args.IsSync;
            };

            element.InteriorChanged += (source, args) =>
            {
                isInteriorEventCalled = true;
                interiorEventSyncValue = args.IsSync;
            };


            await Task.WhenAll(new Task[]
            {
                Task.Run(async () =>
                {
                    await Task.Delay(10);
                    element.Dimension = 1;
                }),
                Task.Run(async () =>
                {
                    await element.RunAsSync(async () =>
                    {
                        await Task.Delay(25);
                        element.Interior = 1;
                    });
                })
            });


            isDimensionEventCalled.Should().BeTrue();
            dimensionEventSyncValue.Should().BeFalse();

            isInteriorEventCalled.Should().BeTrue();
            interiorEventSyncValue.Should().BeTrue();
        }
    }
}
