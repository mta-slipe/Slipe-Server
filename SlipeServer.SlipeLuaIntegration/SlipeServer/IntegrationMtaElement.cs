using SlipeServer.Server.Elements;

namespace SlipeServer.SlipeLuaIntegration.SlipeServer;

public class IntegrationMtaElement
{
    public Element? Element { get; init; }
    public SlipeLua.Shared.Elements.Element? LuaElement { get; init; }

    public static implicit operator IntegrationMtaElement(Element? element)
    {
        return new IntegrationMtaElement()
        {
            Element = element
        };
    }

    public static implicit operator IntegrationMtaElement(SlipeLua.Shared.Elements.Element? element)
    {
        return new IntegrationMtaElement()
        {
            LuaElement = element
        };
    }


    public static implicit operator Element?(IntegrationMtaElement element)
    {
        return element.Element;
    }

    public static implicit operator SlipeLua.Shared.Elements.Element?(IntegrationMtaElement element)
    {
        return element.LuaElement;
    }

    public static implicit operator Vehicle?(IntegrationMtaElement element)
    {
        return element.Element as Vehicle;
    }

    public static implicit operator SlipeLua.Shared.Vehicles.SharedVehicle?(IntegrationMtaElement element)
    {
        return element.LuaElement as SlipeLua.Shared.Vehicles.SharedVehicle;
    }
}
