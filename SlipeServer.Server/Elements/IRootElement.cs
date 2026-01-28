namespace SlipeServer.Server.Elements;

public interface IRootElement : IElement
{
    new RootElement AssociateWith(IMtaServer server);
}
