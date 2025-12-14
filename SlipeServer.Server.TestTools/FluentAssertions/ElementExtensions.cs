using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Server.TestTools.FluentAssertions;

public static class ElementExtensions
{
    public static ElementAssertions Should(this Element element)
    {
        return new ElementAssertions(element);
    }

    public static PedAssertions Should(this Ped ped)
    {
        return new PedAssertions(ped);
    }

    public static PlayerAssertions Should(this Player player)
    {
        return new PlayerAssertions(player);
    }

    public static VehicleAssertions Should(this Vehicle vehicle)
    {
        return new VehicleAssertions(vehicle);
    }
}
