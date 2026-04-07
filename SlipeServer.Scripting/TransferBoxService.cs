using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;

namespace SlipeServer.Scripting;

public class TransferBoxService : ITransferBoxService
{
    private readonly IElementCollection elementCollection;
    private bool isVisible = true;

    public bool IsVisible => this.isVisible;

    public TransferBoxService(IMtaServer server, IElementCollection elementCollection)
    {
        this.elementCollection = elementCollection;
        server.PlayerJoined += player => player.SetTransferBoxVisible(this.isVisible);
    }

    public bool SetVisible(bool visible)
    {
        this.isVisible = visible;
        foreach (var player in this.elementCollection.GetByType<Player>())
            player.SetTransferBoxVisible(visible);
        return true;
    }
}
