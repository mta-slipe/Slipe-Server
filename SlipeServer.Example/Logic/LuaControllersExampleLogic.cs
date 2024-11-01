using SlipeServer.LuaControllers.Commands;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;

namespace SlipeServer.Example.Logic;

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

    private void HandleArgumentErrorOccurred(Player player, Exception exception)
    {
        if (exception is ArgumentOutOfRangeException)
            this.chatBox.OutputTo(player, "Too many or too few arguments");
        else if (exception is LuaControllerArgumentException ex)
        {
            this.chatBox.OutputTo(player, $"Error while executing command, argument at index {ex.Index + 1} expected {ex.ParameterInfo.ParameterType}, got '{ex.Argument}'");
        }
    }
}
