namespace SlipeServer.Server.Elements;

/// <summary>
/// The root element
/// The root element is a special type of element that exists at the top of the element tree.
/// This element does not have any special logic other than that it is the (grand)parent of all elements
/// </summary>
public class RootElement : Element
{
    public override ElementType ElementType => ElementType.Root;

    public new RootElement AssociateWith(IMtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }
}
