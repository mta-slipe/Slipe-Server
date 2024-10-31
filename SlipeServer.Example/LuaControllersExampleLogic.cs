using SlipeServer.LuaControllers.Commands;

namespace SlipeServer.Example;

public class SampleClass
{
    public int Number { get; set; }
}

public class LuaControllersExampleLogic
{
    public LuaControllersExampleLogic(LuaControllerArgumentsMapper mapper)
    {
        mapper.DefineMap<SampleClass>(arg =>
        {
            return new SampleClass
            {
                Number = int.Parse(arg)
            };
        });
    }
}
