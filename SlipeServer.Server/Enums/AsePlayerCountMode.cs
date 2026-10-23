namespace SlipeServer.Server.Enums;

/// <summary>
/// Determines which network wrappers are represented by ASE responses.
/// </summary>
public enum AsePlayerCountMode
{
    /// <summary>
    /// Only players of the queried game port, using its native verification data.
    /// </summary>
    VerifiedPerWrapper,

    /// <summary>
    /// Joined players across all wrappers, without native count verification.
    /// </summary>
    Aggregate
}