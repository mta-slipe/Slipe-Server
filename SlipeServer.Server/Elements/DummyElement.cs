namespace SlipeServer.Server.Elements;

/// <summary>
/// Dummy element
/// An element with no specific purpose, used within resource for resource root elements.
/// </summary>
public class DummyElement : Element
{
    public override ElementType ElementType => ElementType.Dummy;

    public string ElementTypeName { get; set; } = "dummy";

    public new DummyElement AssociateWith(IMtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }
}
