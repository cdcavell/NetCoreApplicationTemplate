namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Computes a cryptographic digest for a canonical application mutation manifest.
/// </summary>
public interface IApplicationMutationManifestHasher
{
    /// <summary>
    /// Gets the stable algorithm identifier written to mutation audit receipts.
    /// </summary>
    string Algorithm { get; }

    /// <summary>
    /// Computes the digest for the supplied canonical manifest.
    /// </summary>
    string ComputeHash(ApplicationMutationManifest manifest);
}
