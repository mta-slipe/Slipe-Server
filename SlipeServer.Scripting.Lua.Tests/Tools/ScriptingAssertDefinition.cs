namespace SlipeServer.Scripting.Lua.Tests.Tools;

public class AssertDataProvider
{
    public List<string> AssertPrints = [];
}

public class ScriptingAssertDefinitions(AssertDataProvider dataProvider)
{
    [ScriptFunctionDefinition("assertPrint")]
    public void AssertPrint(string value) => dataProvider.AssertPrints.Add(value);


}
