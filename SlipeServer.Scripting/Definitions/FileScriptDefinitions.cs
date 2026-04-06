using SlipeServer.Packets.Definitions.Lua;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SlipeServer.Scripting.Definitions;

public class FileScriptDefinitions
{
    private readonly string basePath;

    public FileScriptDefinitions()
    {
        this.basePath = Directory.GetCurrentDirectory();
    }

    private string ResolvePath(string filePath)
    {
        if (System.IO.Path.IsPathRooted(filePath))
            return filePath;

        return System.IO.Path.GetFullPath(System.IO.Path.Combine(this.basePath, filePath));
    }

    [ScriptFunctionDefinition("fileCreate")]
    public ScriptFile FileCreate(string filePath)
    {
        var fullPath = ResolvePath(filePath);
        var dir = System.IO.Path.GetDirectoryName(fullPath);
        if (dir != null)
            Directory.CreateDirectory(dir);

        return new ScriptFile(fullPath, FileMode.Create, FileAccess.ReadWrite);
    }

    [ScriptFunctionDefinition("fileOpen")]
    public ScriptFile? FileOpen(string filePath, bool readOnly = false)
    {
        var fullPath = ResolvePath(filePath);
        if (!File.Exists(fullPath))
            return null;

        var access = readOnly ? FileAccess.Read : FileAccess.ReadWrite;
        return new ScriptFile(fullPath, FileMode.Open, access);
    }

    [ScriptFunctionDefinition("fileClose")]
    public bool FileClose(ScriptFile file)
    {
        file.Stream.Flush();
        file.Dispose();
        return true;
    }

    [ScriptFunctionDefinition("fileRead")]
    public string FileRead(ScriptFile file, int count)
    {
        var buffer = new byte[count];
        var bytesRead = file.Stream.Read(buffer, 0, count);
        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    [ScriptFunctionDefinition("fileWrite")]
    public int FileWrite(ScriptFile file, params string[] strings)
    {
        var totalWritten = 0;
        foreach (var str in strings)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            file.Stream.Write(bytes, 0, bytes.Length);
            totalWritten += bytes.Length;
        }
        return totalWritten;
    }

    [ScriptFunctionDefinition("fileDelete")]
    public bool FileDelete(string filePath)
    {
        var fullPath = ResolvePath(filePath);
        if (!File.Exists(fullPath))
            return false;

        File.Delete(fullPath);
        return true;
    }

    [ScriptFunctionDefinition("fileExists")]
    public bool FileExists(string filePath)
    {
        return File.Exists(ResolvePath(filePath));
    }

    [ScriptFunctionDefinition("fileFlush")]
    public bool FileFlush(ScriptFile file)
    {
        file.Stream.Flush();
        return true;
    }

    [ScriptFunctionDefinition("fileGetContents")]
    public string FileGetContents(ScriptFile file, bool verifyContents = true)
    {
        var originalPosition = file.Stream.Position;
        file.Stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(file.Stream, Encoding.UTF8, leaveOpen: true);
        var contents = reader.ReadToEnd();
        file.Stream.Seek(originalPosition, SeekOrigin.Begin);
        return contents;
    }

    [ScriptFunctionDefinition("fileGetHash")]
    public string? FileGetHash(ScriptFile file, string algorithm, LuaValue? options = null)
    {
        var originalPosition = file.Stream.Position;
        file.Stream.Seek(0, SeekOrigin.Begin);

        try
        {
            if (algorithm.Equals("hmac", StringComparison.OrdinalIgnoreCase))
            {
                var hmacAlgorithm = options?.TableValue?
                    .FirstOrDefault(kv => kv.Key.StringValue == "algorithm").Value?.StringValue ?? "sha256";
                var key = options?.TableValue?
                    .FirstOrDefault(kv => kv.Key.StringValue == "key").Value?.StringValue ?? string.Empty;
                var keyBytes = Encoding.UTF8.GetBytes(key);

                using HMAC hmac = hmacAlgorithm.ToLowerInvariant() switch
                {
                    "sha256" => new HMACSHA256(keyBytes),
                    "sha384" => new HMACSHA384(keyBytes),
                    "sha512" => new HMACSHA512(keyBytes),
                    "sha1" => new HMACSHA1(keyBytes),
                    "md5" => new HMACMD5(keyBytes),
                    _ => new HMACSHA256(keyBytes)
                };

                var hash = hmac.ComputeHash(file.Stream);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }

            HashAlgorithm hasher = algorithm.ToLowerInvariant() switch
            {
                "md5" => MD5.Create(),
                "sha1" => SHA1.Create(),
                "sha256" => SHA256.Create(),
                "sha384" => SHA384.Create(),
                "sha512" => SHA512.Create(),
                _ => throw new ArgumentException($"Unsupported hash algorithm: {algorithm}")
            };

            using (hasher)
            {
                var hash = hasher.ComputeHash(file.Stream);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }
        finally
        {
            file.Stream.Seek(originalPosition, SeekOrigin.Begin);
        }
    }

    [ScriptFunctionDefinition("fileGetPath")]
    public string FileGetPath(ScriptFile file)
    {
        return file.Path;
    }

    [ScriptFunctionDefinition("fileGetPos")]
    public long FileGetPos(ScriptFile file)
    {
        return file.Stream.Position;
    }

    [ScriptFunctionDefinition("fileGetSize")]
    public long FileGetSize(ScriptFile file)
    {
        return file.Stream.Length;
    }

    [ScriptFunctionDefinition("fileIsEOF")]
    public bool FileIsEOF(ScriptFile file)
    {
        return file.Stream.Position >= file.Stream.Length;
    }

    [ScriptFunctionDefinition("fileSetPos")]
    public long FileSetPos(ScriptFile file, long offset)
    {
        return file.Stream.Seek(offset, SeekOrigin.Begin);
    }

    [ScriptFunctionDefinition("fileCopy")]
    public bool FileCopy(string filePath, string copyToFilePath, bool overwrite = false)
    {
        var source = ResolvePath(filePath);
        if (!File.Exists(source))
            return false;

        var destination = ResolvePath(copyToFilePath);
        var dir = System.IO.Path.GetDirectoryName(destination);
        if (dir != null)
            Directory.CreateDirectory(dir);

        File.Copy(source, destination, overwrite);
        return true;
    }

    [ScriptFunctionDefinition("fileRename")]
    public bool FileRename(string filePath, string newFilePath)
    {
        var source = ResolvePath(filePath);
        if (!File.Exists(source))
            return false;

        var destination = ResolvePath(newFilePath);
        var dir = System.IO.Path.GetDirectoryName(destination);
        if (dir != null)
            Directory.CreateDirectory(dir);

        File.Move(source, destination);
        return true;
    }
}
