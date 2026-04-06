using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using System.Security.Cryptography;
using System.Text;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class FileTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void FileCreate_CreatesFileAndReturnsHandle(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                assertPrint(tostring(f ~= nil))
                fileClose(f)
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
    public void FileWrite_WritesContent(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                local bytesWritten = fileWrite(f, "hello")
                assertPrint(tostring(bytesWritten))
                fileClose(f)
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("5");
            File.ReadAllText(fileName, Encoding.UTF8).Should().Be("hello");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileRead_ReadsContent(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                fileWrite(f, "hello world")
                fileSetPos(f, 0)
                local content = fileRead(f, 5)
                assertPrint(content)
                fileClose(f)
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello");
            File.ReadAllText(fileName, Encoding.UTF8).Should().Be("hello world");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileExists_ReturnsTrueForExistingFile(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            File.WriteAllText(fileName, "exists");

            sut.RunLuaScript($$"""
                assertPrint(tostring(fileExists("{{fileName}}")))
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileExists_ReturnsFalseForMissingFile(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(fileExists("this_file_does_not_exist_xyz.txt")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileDelete_DeletesFile(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";

        try
        {
            File.WriteAllText(fileName, "to be deleted");

            sut.RunLuaScript($$"""
                assertPrint(tostring(fileDelete("{{fileName}}")))
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
            File.Exists(fileName).Should().BeFalse();
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileGetSize_ReturnsCorrectSize(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                fileWrite(f, "12345")
                assertPrint(tostring(fileGetSize(f)))
                fileClose(f)
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("5");
            new FileInfo(fileName).Length.Should().Be(5);
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileGetPos_TracksPosition(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                fileWrite(f, "hello")
                assertPrint(tostring(fileGetPos(f)))
                fileClose(f)
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("5");
            File.ReadAllText(fileName, Encoding.UTF8).Should().Be("hello");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileSetPos_MovesPosition(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                fileWrite(f, "hello")
                fileSetPos(f, 2)
                assertPrint(tostring(fileGetPos(f)))
                fileClose(f)
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
            File.ReadAllText(fileName, Encoding.UTF8).Should().Be("hello");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileIsEOF_ReturnsTrueAtEnd(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                fileWrite(f, "hi")
                assertPrint(tostring(fileIsEOF(f)))
                fileClose(f)
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
            File.ReadAllText(fileName, Encoding.UTF8).Should().Be("hi");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileGetContents_ReturnsFullContents(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                fileWrite(f, "full content")
                local contents = fileGetContents(f)
                assertPrint(contents)
                fileClose(f)
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("full content");
            File.ReadAllText(fileName, Encoding.UTF8).Should().Be("full content");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileOpen_OpensExistingFile(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            File.WriteAllText(fileName, "opened");

            sut.RunLuaScript($$"""
                local f = fileOpen("{{fileName}}", true)
                assertPrint(tostring(f ~= nil))
                local content = fileRead(f, 6)
                assertPrint(content)
                fileClose(f)
                """);

            assertDataProvider.AssertPrints.Should().BeEquivalentTo(["true", "opened"], options => options.WithStrictOrdering());
            File.Exists(fileName).Should().BeTrue();
            File.ReadAllText(fileName, Encoding.UTF8).Should().Be("opened");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileCopy_CopiesFile(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var srcName = $"test_src_{Guid.NewGuid()}.txt";
        var dstName = $"test_dst_{Guid.NewGuid()}.txt";
        try
        {
            File.WriteAllText(srcName, "copy me");

            sut.RunLuaScript($$"""
                assertPrint(tostring(fileCopy("{{srcName}}", "{{dstName}}")))
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
            File.Exists(dstName).Should().BeTrue();
            File.ReadAllText(dstName).Should().Be("copy me");
        }
        finally
        {
            if (File.Exists(srcName)) File.Delete(srcName);
            if (File.Exists(dstName)) File.Delete(dstName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileRename_MovesFile(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var srcName = $"test_src_{Guid.NewGuid()}.txt";
        var dstName = $"test_dst_{Guid.NewGuid()}.txt";
        try
        {
            File.WriteAllText(srcName, "move me");

            sut.RunLuaScript($$"""
                assertPrint(tostring(fileRename("{{srcName}}", "{{dstName}}")))
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
            File.Exists(srcName).Should().BeFalse();
            File.Exists(dstName).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(srcName)) File.Delete(srcName);
            if (File.Exists(dstName)) File.Delete(dstName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileGetHash_ReturnsSha256Hash(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                fileWrite(f, "hello")
                local hash = fileGetHash(f, "sha256")
                assertPrint(hash)
                fileClose(f)
                """);

            var expectedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("hello"))).ToLowerInvariant();
            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(expectedHash);
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileGetPath_ReturnsPath(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                local path = fileGetPath(f)
                assertPrint(path)
                fileClose(f)
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().EndWith(fileName);
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FileFlush_FlushesWithoutError(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var fileName = $"test_{Guid.NewGuid()}.txt";
        try
        {
            sut.RunLuaScript($$"""
                local f = fileCreate("{{fileName}}")
                fileWrite(f, "data")
                assertPrint(tostring(fileFlush(f)))
                fileClose(f)
                """);

            assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
            File.ReadAllText(fileName, Encoding.UTF8).Should().Be("data");
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
}
