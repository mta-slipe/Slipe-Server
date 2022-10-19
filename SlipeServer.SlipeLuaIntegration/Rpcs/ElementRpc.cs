using SlipeLua.Shared.Rpc;
using SlipeServer.SlipeLuaIntegration.SlipeServer;

namespace SlipeServer.SlipeLuaIntegration.Rpcs;

public class ElementRpc : BaseRpc
{
    public IntegrationElement? Element { get; set; }

    public ElementRpc()
    {

    }

    public ElementRpc(IntegrationElement element)
    {
        this.Element = element;
    }

    public override void Parse(dynamic value)
    {
        this.Element = (IntegrationElement)GetElement<SlipeLua.Shared.Elements.Element>(value.Element);
    }
}

public class ElementRpc<TServerElementType, TLuaElementType> : BaseRpc
    where TServerElementType : Server.Elements.Element
    where TLuaElementType : SlipeLua.Shared.Elements.Element
{
    public IntegrationElement<TServerElementType, TLuaElementType>? Element { get; set; }

    public ElementRpc()
    {

    }

    public ElementRpc(TServerElementType element)
    {
        this.Element = element;
    }

    public ElementRpc(TLuaElementType element)
    {
        this.Element = element;
    }

    public override void Parse(dynamic value)
    {
        this.Element = GetElement<TLuaElementType>(value.Element);
    }
}
