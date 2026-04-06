using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

if (args.Contains("--help") || args.Contains("-h"))
{
    PrintHelp();
    return;
}

string outputDir = ResolveOutputDir(args);
BuildSource[] allSources = GetAllSources();
BuildSource[] sources = FilterSources(allSources, args);

Console.WriteLine($"Output directory: {Path.GetFullPath(outputDir)}");
Console.WriteLine();

using var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.ParseAdd("NetDllUpdater/1.0");
http.Timeout = TimeSpan.FromMinutes(30);

int succeeded = 0, failed = 0;

foreach (BuildSource source in sources)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"[{source.Name}]");
    Console.ResetColor();

    try
    {
        await ProcessSourceAsync(source, outputDir, http);
        succeeded++;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ERROR: {ex.Message}");
        Console.ResetColor();
        failed++;
    }

    Console.WriteLine();
}

Console.WriteLine($"Done -- {succeeded} succeeded, {failed} failed.");

static BuildSource[] GetAllSources() =>
[
    new("linux-x64",     "Linux x64",     BuildUrls.LinuxX64,   "x64",   ArchiveFormat.TarGz,   ["net.so"]),
    new("linux-x86",     "Linux x86",     BuildUrls.LinuxX86,   "x86",   ArchiveFormat.TarGz,   ["net.so"]),
    new("linux-arm64",   "Linux ARM64",   BuildUrls.LinuxArm64, "arm64", ArchiveFormat.TarGz,   ["net.so"]),
    new("linux-arm32",   "Linux ARM32",   BuildUrls.LinuxArm32, "arm32", ArchiveFormat.TarGz,   ["net.so"]),
    new("windows-arm64", "Windows ARM64", BuildUrls.WinArm64,   "arm64", ArchiveFormat.Rar,     ["net.dll", "pthread.dll"]),
    new("windows-x64",   "Windows x64",   BuildUrls.WinX64,     "x64",   ArchiveFormat.NsisExe, ["net.dll", "pthread.dll"]),
    new("windows-x86",       "Windows x86",         BuildUrls.WinX86,       "x86",   ArchiveFormat.NsisExe, ["net.dll", "pthread.dll"]),
    new("windows-x64-debug", "Windows x64 (debug)", BuildUrls.WinX64Debug,  "x64",   ArchiveFormat.Direct,  ["net_d.dll"]),
    new("windows-x86-debug", "Windows x86 (debug)", BuildUrls.WinX86Debug,  "x86",   ArchiveFormat.Direct,  ["net_d.dll"]),
];

static async Task ProcessSourceAsync(BuildSource source, string outputDir, HttpClient http)
{
    string tempDir = Path.Combine(Path.GetTempPath(), "NetDllUpdater", source.Key);
    Directory.CreateDirectory(tempDir);

    try
    {
        string targetDir = Path.Combine(outputDir, source.BinDir);
        Directory.CreateDirectory(targetDir);

        if (source.Format == ArchiveFormat.Direct)
        {
            await DownloadDirectAsync(source.Url, source.Files[0], targetDir, http);
            return;
        }

        string archivePath = await DownloadAsync(source.Url, tempDir, http);

        string extractDir = Path.Combine(tempDir, "extracted");
        Directory.CreateDirectory(extractDir);

        await ExtractAsync(source.Format, archivePath, extractDir);

        CopyFiles(source, extractDir, targetDir);
    }
    finally
    {
        try { Directory.Delete(tempDir, recursive: true); } catch { }
    }
}

static async Task<string> DownloadAsync(string url, string tempDir, HttpClient http)
{
    string fileName = Path.GetFileName(new Uri(url).LocalPath);
    string destPath = Path.Combine(tempDir, fileName);

    using HttpResponseMessage response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();

    long? total = response.Content.Headers.ContentLength;

    await using Stream body = await response.Content.ReadAsStreamAsync();
    await using FileStream file = File.Create(destPath);

    byte[] buffer = new byte[81920];
    long downloaded = 0;
    int lastPct = -1;
    int read;

    while ((read = await body.ReadAsync(buffer)) > 0)
    {
        await file.WriteAsync(buffer.AsMemory(0, read));
        downloaded += read;

        if (total.HasValue)
        {
            int pct = (int)(downloaded * 100 / total.Value);
            if (pct != lastPct)
            {
                Console.Write($"\r  Downloading {fileName}... {pct}%   ");
                lastPct = pct;
            }
        }
        else
        {
            Console.Write($"\r  Downloading {fileName}... {FormatBytes(downloaded)}   ");
        }
    }

    Console.WriteLine($"\r  Downloaded  {fileName} ({FormatBytes(downloaded)})   ");
    return destPath;
}

static async Task DownloadDirectAsync(string url, string fileName, string targetDir, HttpClient http)
{
    string tempDir = Path.Combine(Path.GetTempPath(), "NetDllUpdater", "direct");
    Directory.CreateDirectory(tempDir);
    string downloaded = await DownloadAsync(url, tempDir, http);
    string dest = Path.Combine(targetDir, fileName);
    File.Copy(downloaded, dest, overwrite: true);
    Console.WriteLine($"  Copied {fileName} -> {dest}");
}

static Task ExtractAsync(ArchiveFormat format, string archivePath, string extractDir) =>
    format switch
    {
        ArchiveFormat.TarGz   => ExtractTarGzAsync(archivePath, extractDir),
        ArchiveFormat.Rar     => ExtractRarAsync(archivePath, extractDir),
        ArchiveFormat.NsisExe => ExtractExeAsync(archivePath, extractDir),
        _                     => throw new NotSupportedException($"Unsupported format: {format}"),
    };

static async Task ExtractTarGzAsync(string archivePath, string extractDir)
{
    Console.Write("  Extracting tar.gz...");
    await using FileStream fs = File.OpenRead(archivePath);
    await using GZipStream gz = new(fs, CompressionMode.Decompress);
    await TarFile.ExtractToDirectoryAsync(gz, extractDir, overwriteFiles: true);
    Console.WriteLine(" done.");
}

static Task ExtractRarAsync(string archivePath, string extractDir)
{
    Console.Write("  Extracting rar...");
    using IArchive archive = ArchiveFactory.Open(archivePath);
    using IReader reader = archive.ExtractAllEntries();
    reader.WriteAllToDirectory(extractDir, new ExtractionOptions
    {
        ExtractFullPath = true,
        Overwrite = true,
    });
    Console.WriteLine(" done.");
    return Task.CompletedTask;
}

static async Task ExtractExeAsync(string archivePath, string extractDir)
{
    Console.Write("  Extracting installer via 7-zip...");

    string? sevenZip = Find7Zip()
        ?? throw new InvalidOperationException(
            "7-Zip not found. Install it from https://7-zip.org/ or add 7z to PATH.");

    using Process proc = Process.Start(new ProcessStartInfo(sevenZip, $"x \"{archivePath}\" -o\"{extractDir}\" -y")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
    }) ?? throw new InvalidOperationException("Failed to start 7-zip process.");

    await proc.WaitForExitAsync();

    if (proc.ExitCode != 0)
    {
        string err = await proc.StandardError.ReadToEndAsync();
        throw new InvalidOperationException($"7-zip failed (exit code {proc.ExitCode}): {err.Trim()}");
    }

    Console.WriteLine(" done.");
}

static void CopyFiles(BuildSource source, string extractDir, string targetDir)
{
    foreach (string fileName in source.Files)
    {
        List<string> matches = Directory
            .EnumerateFiles(extractDir, fileName, SearchOption.AllDirectories)
            .OrderBy(ScoreFilePath)
            .ToList();

        if (matches.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  WARNING: {fileName} not found in archive, skipping.");
            Console.ResetColor();
            continue;
        }

        if (matches.Count > 1)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"  NOTE: Multiple matches for {fileName}, using: {matches[0]}");
            Console.ResetColor();
        }

        string dest = Path.Combine(targetDir, fileName);
        File.Copy(matches[0], dest, overwrite: true);
        Console.WriteLine($"  Copied {fileName} -> {dest}");
    }
}

static int ScoreFilePath(string path)
{
    if (path.Contains("server", StringComparison.OrdinalIgnoreCase)) return 0;
    if (path.Contains("client", StringComparison.OrdinalIgnoreCase)) return 2;
    return 1;
}

static string? Find7Zip()
{
    string[] candidates =
    [
        "7z",
        @"C:\Program Files\7-Zip\7z.exe",
        @"C:\Program Files (x86)\7-Zip\7z.exe",
    ];

    foreach (string candidate in candidates)
    {
        try
        {
            using Process? p = Process.Start(new ProcessStartInfo(candidate, "i")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            });
            if (p is not null) { p.WaitForExit(); return candidate; }
        }
        catch { }
    }

    return null;
}

static string ResolveOutputDir(string[] args)
{
    int idx = Array.IndexOf(args, "--output");
    if (idx >= 0 && idx + 1 < args.Length)
        return args[idx + 1];

    string[] roots = [Directory.GetCurrentDirectory(), AppContext.BaseDirectory];

    foreach (string root in roots)
    {
        string dir = root;
        for (int i = 0; i <= 4; i++)
        {
            if (Directory.Exists(Path.Combine(dir, "SlipeServer.Net")))
                return Path.Combine(dir, "SlipeServer.Net", "Binaries");

            string? parent = Path.GetDirectoryName(dir);
            if (parent is null) break;
            dir = parent;
        }
    }

    return Path.Combine(Directory.GetCurrentDirectory(), "..", "SlipeServer.Net", "Binaries");
}

static BuildSource[] FilterSources(BuildSource[] all, string[] args)
{
    string[] platforms = args.Where(a => !a.StartsWith('-')).ToArray();
    if (platforms.Length == 0)
        return all;

    return all
        .Where(s => platforms.Any(p =>
            s.Key.Equals(p, StringComparison.OrdinalIgnoreCase) ||
            s.Name.Equals(p, StringComparison.OrdinalIgnoreCase)))
        .ToArray();
}

static string FormatBytes(long bytes) => bytes switch
{
    < 1024        => $"{bytes} B",
    < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
    _             => $"{bytes / (1024.0 * 1024.0):F1} MB",
};

static void PrintHelp()
{
    Console.WriteLine("""
        NetDllUpdater -- Downloads MTA:SA net binaries into SlipeServer.Net/Binaries

        Usage:
          NetDllUpdater [options] [platforms...]

        Options:
          --output <dir>   Override the output Binaries directory
          --help, -h       Show this help

        Platforms (default: all):
          linux-x64         Linux x64   (.tar.gz) -> x64/net.so
          linux-x86         Linux x86   (.tar.gz) -> x86/net.so
          linux-arm64       Linux ARM64 (.tar.gz) -> arm64/net.so
          linux-arm32       Linux ARM32 (.tar.gz) -> arm32/net.so
          windows-arm64     Windows ARM64 (.rar)  -> arm64/net.dll
          windows-x64         Windows x64  (.exe)   -> x64/net.dll   [requires 7-Zip]
          windows-x86         Windows x86  (.exe)   -> x86/net.dll   [requires 7-Zip]
          windows-x64-debug   Windows x64 debug     -> x64/net_d.dll
          windows-x86-debug   Windows x86 debug     -> x86/net_d.dll

        Notes:
          * Windows .exe installers require 7-Zip (https://7-zip.org/).
            Install it or add 7z to PATH before running those targets.

        Examples:
          NetDllUpdater                           # Download all platforms
          NetDllUpdater linux-x64 linux-x86       # Only Linux x64 and x86
          NetDllUpdater windows-x64               # Only Windows x64
          NetDllUpdater --output /path/to/Binaries linux-arm64
        """);
}

enum ArchiveFormat { TarGz, Rar, NsisExe, Direct }

record BuildSource(
    string Key,
    string Name,
    string Url,
    string BinDir,
    ArchiveFormat Format,
    string[] Files);

static class BuildUrls
{
    public const string LinuxX64   = "https://nightly.multitheftauto.com/multitheftauto_linux_x64-1.6.0-rc-23975.tar.gz";
    public const string LinuxX86   = "https://nightly.multitheftauto.com/multitheftauto_linux-1.6.0-rc-23967.tar.gz";
    public const string LinuxArm64 = "https://nightly.multitheftauto.com/multitheftauto_linux_arm64-1.6.0-rc-23832.tar.gz";
    public const string LinuxArm32 = "https://nightly.multitheftauto.com/multitheftauto_linux_arm-1.6.0-rc-23757.tar.gz";
    public const string WinArm64   = "https://nightly.multitheftauto.com/mtasa_windows_arm64-1.6.0-rc-24025.rar";
    public const string WinX64      = "https://nightly.multitheftauto.com/mtasa_x64-1.6-rc-24025-20260402.exe";
    public const string WinX86      = "https://nightly.multitheftauto.com/mtasa-1.6-rc-24034-20260404.exe";
    public const string WinX64Debug = "https://mirror-cdn.multitheftauto.com/bdata/net_64.dll";
    public const string WinX86Debug = "https://mirror-cdn.multitheftauto.com/bdata/net.dll";
}
