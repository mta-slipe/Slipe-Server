namespace SlipeServer.Scripting;

public interface ITransferBoxService
{
    bool IsVisible { get; }
    bool SetVisible(bool visible);
}
