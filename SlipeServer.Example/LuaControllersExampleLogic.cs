using SlipeServer.LuaControllers.Commands;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;

namespace SlipeServer.Example;

public class SampleClass
{
    public int Number { get; set; }
}

public class LuaControllersExampleLogic
{
    private readonly IElementCollection elementCollection;
    private readonly ChatBox chatBox;

    public LuaControllersExampleLogic(LuaControllerArgumentsMapper mapper, IElementCollection elementCollection, ChatBox chatBox)
    {
        mapper.DefineMap<SampleClass>(arg =>
        {
            return new SampleClass
            {
                Number = int.Parse(arg)
            };
        });
        mapper.DefineMap<Player>(arg =>
        {
            return elementCollection.GetByType<Player>().Where(x => x.Name.Contains(arg)).FirstOrDefault();
        });

        mapper.ArgumentErrorOccurred += HandleArgumentErrorOccurred;
        this.elementCollection = elementCollection;
        this.chatBox = chatBox;
    }

    private void HandleArgumentErrorOccurred(Player player, LuaControllerArgumentException ex)
    {
        if(ex.InnerException is ArgumentOutOfRangeException)
        {
            this.chatBox.OutputTo(player, "Too many or too few arguments");
        } else
        {
            this.chatBox.OutputTo(player, "Error while executing command");
        }
    }
}
