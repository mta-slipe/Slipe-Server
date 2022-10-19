using SlipeServer.Server.Elements;

namespace SlipeServer.SlipeLuaIntegration.SlipeServer;

public class IntegrationElement
{
    public Element? Element { get; init; }
    public SlipeLua.Shared.Elements.Element? LuaElement { get; init; }


    public static implicit operator IntegrationElement(Element? element)
    {
        return new IntegrationElement()
        {
            Element = element
        };
    }

    public static implicit operator IntegrationElement(SlipeLua.Shared.Elements.Element? element)
    {
        return new IntegrationElement()
        {
            LuaElement = element
        };
    }

    public static implicit operator Element?(IntegrationElement element)
    {
        return element.Element;
    }

    public static implicit operator SlipeLua.Shared.Elements.Element?(IntegrationElement element)
    {
        return element.LuaElement;
    }

    public T? AsLuaElement<T>() where T : SlipeLua.Shared.Elements.Element
    {
        return (T?)this.LuaElement;
    }

    public T? AsServerElement<T>() where T : Element
    {
        return (T?)this.Element;
    }
}


public class IntegrationElement<TSlipeServerElement, TSlipeLuaElement> : IntegrationElement
    where TSlipeServerElement: Element
    where TSlipeLuaElement: SlipeLua.Shared.Elements.Element
{
    public new TSlipeServerElement? Element
    {
        get => base.Element as TSlipeServerElement;
        init => base.Element = value;
    }

    public new TSlipeLuaElement? LuaElement
    {
        get => base.LuaElement as TSlipeLuaElement;
        init => base.LuaElement = value;
    }


    public static implicit operator IntegrationElement<TSlipeServerElement, TSlipeLuaElement>(TSlipeServerElement? element)
    {
        return new IntegrationElement<TSlipeServerElement, TSlipeLuaElement>()
        {
            Element = element
        };
    }

    public static implicit operator IntegrationElement<TSlipeServerElement, TSlipeLuaElement>(TSlipeLuaElement? element)
    {
        return new IntegrationElement<TSlipeServerElement, TSlipeLuaElement>()
        {
            LuaElement = element
        };
    }

    public static implicit operator TSlipeServerElement?(IntegrationElement<TSlipeServerElement, TSlipeLuaElement> element)
    {
        return element.Element;
    }

    public static implicit operator TSlipeLuaElement?(IntegrationElement<TSlipeServerElement, TSlipeLuaElement> element)
    {
        return element.LuaElement;
    }

    public new T? AsLuaElement<T>() where T : TSlipeLuaElement
    {
        return (T?)this.LuaElement;
    }

    public new T? AsServerElement<T>() where T : TSlipeServerElement
    {
        return (T?)this.Element;
    }
}
