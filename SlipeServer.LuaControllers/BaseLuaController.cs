using SlipeServer.LuaControllers.Contexts;

namespace SlipeServer.LuaControllers;

public class BaseLuaController
{
    private readonly AsyncLocal<LuaEventContext?> context = new();

    public LuaEventContext Context
    {
        get
        {
            if (this.context.Value == null)
                throw new Exception("Can not access BaseLuaController.Context outside of event handling methods.");

            return this.context.Value;
        }
    }

    internal void SetContext(LuaEventContext? context)
    {
        this.context.Value = context;
    }
}
