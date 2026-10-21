using System;
using System.Globalization;

namespace SlipeServer.Net.Wrappers.Enums;

/// <summary>
/// Which ASE version family a peer expects, derived from the version-type digit of the client's
/// net-layer version string (the <c>9</c> in <c>1.6.0-9.24149.0</c>).
///
/// <para>
/// MTA compiles its ASE version string from the build type: builds with
/// <c>MTA_DM_PUBLIC_CONNECTIONS</c> defined (release <c>0x09</c>, untested <c>0x07</c>) advertise
/// <c>"1.6"</c>, every other build (unstable <c>0x05</c>, experimental <c>0x03</c>, custom
/// <c>0x01</c>) advertises <c>"1.6n"</c>. A connecting client compares the string a server
/// advertises against its own and aborts the connection on a mismatch, so a server that wants to
/// accept both builds has to answer each peer with the family that peer was compiled for.
/// </para>
/// </summary>
public enum MtaPeerBuildType
{
    /// <summary>Public-connection build (version type <c>9</c> or <c>7</c>): expects <c>"1.6"</c>.</summary>
    Release,

    /// <summary>Non-public build (version type <c>5</c>, <c>3</c> or <c>1</c>): expects <c>"1.6n"</c>.</summary>
    Debug
}

/// <summary>
/// Reads the version-type digit out of a client's net-layer version string.
/// </summary>
public static class MtaPeerBuildTypeParser
{
    /// <summary>
    /// Parses the version-type field of a string shaped
    /// <c>major.minor.patch-type.build.revision</c>, such as <c>1.6.0-9.24149.0</c> (release) or
    /// <c>1.6.0-5.24155.0</c> (unstable, which is what a debug client reports).
    /// </summary>
    /// <param name="playerVersion">The client version string read from its join data.</param>
    /// <param name="build">The version family, when the string carries a recognisable type.</param>
    /// <returns><see langword="false"/> for an empty, malformed, or unknown version type.</returns>
    public static bool TryParsePlayerVersion(string? playerVersion, out MtaPeerBuildType build)
    {
        build = default;

        if (string.IsNullOrEmpty(playerVersion))
            return false;

        int dash = playerVersion.IndexOf('-');
        if (dash < 0 || dash + 1 >= playerVersion.Length)
            return false;

        int end = playerVersion.IndexOf('.', dash + 1);
        if (end < 0)
            end = playerVersion.Length;

        if (!int.TryParse(playerVersion.AsSpan(dash + 1, end - dash - 1), NumberStyles.None,
                CultureInfo.InvariantCulture, out int versionType))
            return false;

        switch (versionType)
        {
            case 0x09:
            case 0x07:
                build = MtaPeerBuildType.Release;
                return true;
            case 0x05:
            case 0x03:
            case 0x01:
                build = MtaPeerBuildType.Debug;
                return true;
            default:
                return false;
        }
    }
}
