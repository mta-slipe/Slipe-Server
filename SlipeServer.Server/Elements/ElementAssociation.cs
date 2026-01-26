namespace SlipeServer.Server.Elements;

public class ElementAssociation
{
    public Element Element { get; }
    public Player? Player { get; }
    public IMtaServer? Server { get; set; }
    public bool IsGlobal => this.Server != null;

    public ElementAssociation(Element element, Player player)
    {
        this.Element = element;
        this.Player = player;
    }

    public ElementAssociation(Element element, IMtaServer server)
    {
        this.Element = element;
        this.Server = server;
    }
}
