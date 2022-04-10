using FluentAssertions;
using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Elements;
public class ElementPoolTests
{
    [Fact]
    public void GetOrCreate_CreatesWhenEmpty()
    {
        var pool = new ElementPool<Element>();

        bool isCalled = false;
        pool.GetOrCreateElement(() =>
        {
            isCalled = true;
            return new Element();
        });

        isCalled.Should().BeTrue();
    }

    [Fact]
    public void GetOrCreate_DoesNotCallCleanWhenEmpty()
    {
        var pool = new ElementPool<Element>();

        bool isCalled = false;
        pool.GetOrCreateElement(() => new Element(), element => isCalled = true);

        isCalled.Should().BeFalse();
    }

    [Fact]
    public void GetOrCreate_DoesNotCreateWhenContainingElement()
    {
        var pool = new ElementPool<Element>();

        pool.ReturnElement(new Element());

        bool isCalled = false;
        pool.GetOrCreateElement(() =>
        {
            isCalled = true;
            return new Element();
        });

        isCalled.Should().BeFalse();
    }

    [Fact]
    public void GetOrCreate_CallsCleanWhenReturningDirtyElement()
    {
        var pool = new ElementPool<Element>();

        pool.ReturnElement(new Element());

        bool isCalled = false;
        pool.GetOrCreateElement(() => new Element(), element => isCalled = true);

        isCalled.Should().BeTrue();
    }

    [Fact]
    public void StoresMaximumAmountOfPooledElements()
    {
        var pool = new ElementPool<Element>(5);

        for (int i = 0; i < 10; i++)
            pool.ReturnElement(new Element());

        var callCount = 0;
        for (int i = 0; i < 10; i++)
            pool.GetOrCreateElement(() =>
            {
                callCount++;
                return new Element();
            });

        callCount.Should().Be(5);
    }

    [Fact]
    public void ReturnsOnDestroyWhenTrue()
    {
        var pool = new ElementPool<Element>(returnsWhenDestroyed: true);

        var element1 = pool.GetOrCreateElement(() => new Element());
        element1.Destroy();

        var element2 = pool.GetOrCreateElement(() => new Element());

        element2.Should().Be(element1);
    }

    [Fact]
    public void DoesNotReturnOnDestroyWhenFalse()
    {
        var pool = new ElementPool<Element>(returnsWhenDestroyed: false);

        var element1 = pool.GetOrCreateElement(() => new Element());
        element1.Destroy();

        var element2 = pool.GetOrCreateElement(() => new Element());

        element2.Should().NotBe(element1);
    }
}
