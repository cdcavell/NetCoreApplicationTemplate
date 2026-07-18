using System.Security.Cryptography;
using System.Text;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Computes SHA-256 digests for canonical application mutation manifests.
/// </summary>
public sealed class Sha256ApplicationMutationManifestHasher : IApplicationMutationManifestHasher
{
    /// <inheritdoc />
    public string Algorithm => "SHA-256";

    /// <inheritdoc />
    public string ComputeHash(ApplicationMutationManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(manifest.CanonicalJson));
        return Convert.ToHexString(digest);
    }
}
