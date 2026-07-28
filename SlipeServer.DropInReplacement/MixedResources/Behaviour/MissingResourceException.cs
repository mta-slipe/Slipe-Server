namespace SlipeServer.DropInReplacement.MixedResources.Behaviour;

/// <summary>
/// Thrown when a required resource include cannot be found and missing includes are not allowed.
/// </summary>
public class MissingResourceException : Exception
{
    public string ResourceName { get; }
    public string RequiredBy { get; }

    public MissingResourceException(string resourceName, string requiredBy)
        : base($"Required resource '{resourceName}' (needed by '{requiredBy}') was not found.")
    {
        ResourceName = resourceName;
        RequiredBy = requiredBy;
    }
}
