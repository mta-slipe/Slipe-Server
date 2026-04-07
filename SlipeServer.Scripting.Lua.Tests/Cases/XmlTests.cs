using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class XmlTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void XmlCreateFile_CreatesRootNode(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.xml";
        try
        {
            sut.RunLuaScript($$"""
                local root = xmlCreateFile("{{fileName}}", "config")
                assertPrint(tostring(root ~= nil))
                assertPrint(xmlNodeGetName(root))
                xmlUnloadFile(root)
                """);

            assertDataProvider.AssertPrints.Should().HaveCount(2);
            assertDataProvider.AssertPrints[0].Should().Be("true");
            assertDataProvider.AssertPrints[1].Should().Be("config");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlSaveFile_WritesFileToDisk(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.xml";
        try
        {
            sut.RunLuaScript($$"""
                local root = xmlCreateFile("{{fileName}}", "config")
                local result = xmlSaveFile(root)
                assertPrint(tostring(result))
                xmlUnloadFile(root)
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
            File.Exists(fileName).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlLoadFile_LoadsExistingFile(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.xml";
        try
        {
            File.WriteAllText(fileName, "<settings><item key=\"value\"/></settings>");

            sut.RunLuaScript($$"""
                local root = xmlLoadFile("{{fileName}}")
                assertPrint(tostring(root ~= nil))
                assertPrint(xmlNodeGetName(root))
                xmlUnloadFile(root)
                """);

            assertDataProvider.AssertPrints.Should().HaveCount(2);
            assertDataProvider.AssertPrints[0].Should().Be("true");
            assertDataProvider.AssertPrints[1].Should().Be("settings");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlLoadFile_ReturnsNilForMissingFile(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadFile("nonexistent_file.xml")
            assertPrint(tostring(root == nil))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlLoadString_ParsesXmlString(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<animals test='x'><wolf name='timmy'/></animals>")
            assertPrint(tostring(root ~= nil))
            assertPrint(xmlNodeGetName(root))
            assertPrint(xmlNodeGetAttribute(root, "test"))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(3);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("animals");
        assertDataProvider.AssertPrints[2].Should().Be("x");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlCreateChild_AddsChildNode(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.xml";
        try
        {
            sut.RunLuaScript($$"""
                local root = xmlCreateFile("{{fileName}}", "config")
                local child = xmlCreateChild(root, "item")
                assertPrint(tostring(child ~= nil))
                assertPrint(xmlNodeGetName(child))
                xmlSaveFile(root)
                xmlUnloadFile(root)
                """);

            assertDataProvider.AssertPrints.Should().HaveCount(2);
            assertDataProvider.AssertPrints[0].Should().Be("true");
            assertDataProvider.AssertPrints[1].Should().Be("item");

            var content = File.ReadAllText(fileName);
            content.Should().Contain("<item");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlDestroyNode_RemovesNode(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<config><item/></config>")
            local child = xmlNodeGetChildren(root, 0)
            local result = xmlDestroyNode(child)
            assertPrint(tostring(result))
            local children = xmlNodeGetChildren(root)
            assertPrint(tostring(#children))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlFindChild_FindsChildByTagName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<config><item name='first'/><item name='second'/></config>")
            local child = xmlFindChild(root, "item", 1)
            assertPrint(tostring(child ~= nil))
            assertPrint(xmlNodeGetAttribute(child, "name"))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("second");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeGetAttribute_ReturnsAttributeValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<car model='528' posX='123.4'/>")
            assertPrint(xmlNodeGetAttribute(root, "model"))
            assertPrint(xmlNodeGetAttribute(root, "posX"))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("528");
        assertDataProvider.AssertPrints[1].Should().Be("123.4");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeGetAttributes_ReturnsAllAttributes(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<item a='1' b='2'/>")
            local attrs = xmlNodeGetAttributes(root)
            assertPrint(attrs["a"])
            assertPrint(attrs["b"])
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("1");
        assertDataProvider.AssertPrints[1].Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeGetChildren_ReturnsAllChildren(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<config><a/><b/><c/></config>")
            local children = xmlNodeGetChildren(root)
            assertPrint(tostring(#children))
            assertPrint(xmlNodeGetName(children[1]))
            assertPrint(xmlNodeGetName(children[2]))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(3);
        assertDataProvider.AssertPrints[0].Should().Be("3");
        assertDataProvider.AssertPrints[1].Should().Be("a");
        assertDataProvider.AssertPrints[2].Should().Be("b");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeGetChildren_WithIndex_ReturnsSpecificChild(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<config><a/><b/><c/></config>")
            local child = xmlNodeGetChildren(root, 1)
            assertPrint(xmlNodeGetName(child))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("b");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeGetName_ReturnsTagName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<myElement/>")
            assertPrint(xmlNodeGetName(root))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("myElement");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeGetParent_ReturnsParentNode(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<config><item/></config>")
            local child = xmlNodeGetChildren(root, 0)
            local parent = xmlNodeGetParent(child)
            assertPrint(tostring(parent ~= nil))
            assertPrint(xmlNodeGetName(parent))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("config");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeGetParent_ReturnsNilForRoot(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<config/>")
            local parent = xmlNodeGetParent(root)
            assertPrint(tostring(parent == nil))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeGetValue_ReturnsTextContent(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<message>Hello World</message>")
            assertPrint(xmlNodeGetValue(root))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("Hello World");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeSetAttribute_SetsStringAttribute(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<item/>")
            xmlNodeSetAttribute(root, "color", "red")
            assertPrint(xmlNodeGetAttribute(root, "color"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("red");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeSetAttribute_SetsNumericAttribute(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<item/>")
            xmlNodeSetAttribute(root, "count", 42)
            assertPrint(xmlNodeGetAttribute(root, "count"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("42");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeSetName_RenamesNode(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<oldName/>")
            local result = xmlNodeSetName(root, "newName")
            assertPrint(tostring(result))
            assertPrint(xmlNodeGetName(root))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("newName");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlNodeSetValue_SetsTextContent(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = xmlLoadString("<message/>")
            local result = xmlNodeSetValue(root, "Hello World")
            assertPrint(tostring(result))
            assertPrint(xmlNodeGetValue(root))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("Hello World");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlCopyFile_CopiesNodeToNewPath(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var originalFile = $"test_{Guid.NewGuid()}.xml";
        var copyFile = $"test_{Guid.NewGuid()}.xml";
        try
        {
            File.WriteAllText(originalFile, "<config><item key='val'/></config>");

            sut.RunLuaScript($$"""
                local root = xmlLoadFile("{{originalFile}}")
                local copy = xmlCopyFile(root, "{{copyFile}}")
                assertPrint(tostring(copy ~= nil))
                assertPrint(xmlNodeGetName(copy))
                xmlSaveFile(copy)
                xmlUnloadFile(root)
                xmlUnloadFile(copy)
                """);

            assertDataProvider.AssertPrints.Should().HaveCount(2);
            assertDataProvider.AssertPrints[0].Should().Be("true");
            assertDataProvider.AssertPrints[1].Should().Be("config");
            File.Exists(copyFile).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(originalFile))
                File.Delete(originalFile);
            if (File.Exists(copyFile))
                File.Delete(copyFile);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void XmlRoundTrip_SaveAndReload(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.xml";
        try
        {
            sut.RunLuaScript($$"""
                local root = xmlCreateFile("{{fileName}}", "config")
                local child = xmlCreateChild(root, "item")
                xmlNodeSetAttribute(child, "name", "test")
                xmlNodeSetAttribute(child, "value", "42")
                xmlSaveFile(root)
                xmlUnloadFile(root)

                local loaded = xmlLoadFile("{{fileName}}")
                local item = xmlFindChild(loaded, "item", 0)
                assertPrint(xmlNodeGetAttribute(item, "name"))
                assertPrint(xmlNodeGetAttribute(item, "value"))
                xmlUnloadFile(loaded)
                """);

            assertDataProvider.AssertPrints.Should().HaveCount(2);
            assertDataProvider.AssertPrints[0].Should().Be("test");
            assertDataProvider.AssertPrints[1].Should().Be("42");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
}
