namespace SlipeServer.Scripting.Lua.Tests.Tools;

public class AssertDataProvider
{
    public List<string> AssertPrints = [];
    public List<string> ScriptErrors = [];
}

public class ScriptingAssertDefinitions(AssertDataProvider dataProvider)
{
    public AssertDataProvider DataProvider => dataProvider;

    [ScriptFunctionDefinition("assertPrint")]
    public void AssertPrint(string value) => dataProvider.AssertPrints.Add(value);
}
