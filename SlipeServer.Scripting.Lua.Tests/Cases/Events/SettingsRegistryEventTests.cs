using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class SettingsRegistryEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnSettingChange_FiresWhenSettingIsSet(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEventHandler("onSettingChange", getRootElement(), function(setting, oldValue, newValue)
                assertPrint(setting)
            end)
            set("myKey", "hello")
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("myKey");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnSettingChange_ProvidesOldAndNewValuesAsJson(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            set("myKey", "first")
            addEventHandler("onSettingChange", getRootElement(), function(setting, oldValue, newValue)
                assertPrint(fromJSON(oldValue))
                assertPrint(fromJSON(newValue))
            end)
            set("myKey", "second")
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("first");
        assertDataProvider.AssertPrints[1].Should().Be("second");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnSettingChange_OldValueIsJsonNull_WhenKeyIsNew(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEventHandler("onSettingChange", getRootElement(), function(setting, oldValue, newValue)
                assertPrint(oldValue)
                assertPrint(fromJSON(newValue))
            end)
            set("freshKey", "value")
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("null");
        assertDataProvider.AssertPrints[1].Should().Be("value");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnSettingChange_IncludesOldValueAsJson_WhenOverwriting(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            set("overwriteKey", "initial")
            addEventHandler("onSettingChange", getRootElement(), function(setting, oldValue, newValue)
                assertPrint(fromJSON(oldValue))
                assertPrint(fromJSON(newValue))
            end)
            set("overwriteKey", "updated")
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("initial");
        assertDataProvider.AssertPrints[1].Should().Be("updated");
    }
}
