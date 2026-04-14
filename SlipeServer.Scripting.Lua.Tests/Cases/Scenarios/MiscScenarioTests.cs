using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Scenarios;

public class MiscScenarioTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void AddingStringMethod_WorksAsIntended(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""            
            function string:split(separator)
            	if separator == '.' then
            		separator = '%.'
            	end
            	local result = {}
            	for part in self:gmatch('(.-)' .. separator) do
            		result[#result+1] = part
            	end
            	result[#result+1] = self:match('.*' .. separator .. '(.*)$') or self
            	return result
            end

            local foo = "hey|there"
            local bar = foo:split("|")

            assertPrint(bar[1])
            assertPrint(bar[2])
            """);

        assertDataProvider.AssertPrints.Should().BeEquivalentTo(["hey", "there"]);
    }
}
