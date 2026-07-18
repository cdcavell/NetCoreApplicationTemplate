using System.Security.Cryptography;
using System.Text;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Verifies retained application audit records against a canonical mutation manifest digest.
/// </summary>
public sealed class ApplicationMutationManifestVerifier(
    IApplicationMutationManifestBuilder manifestBuilder,
    IApplicationMutationManifestHasher manifestHasher)
    : IApplicationMutationManifestVerifier
{
    private readonly IApplicationMutationManifestBuilder _manifestBuilder =
        manifestBuilder ?? throw new ArgumentNullException(nameof(manifestBuilder));
    private readonly IApplicationMutationManifestHasher _manifestHasher =
        manifestHasher ?? throw new ArgumentNullException(nameof(manifestHasher));

    /// <inheritdoc />
    public bool Verify(
        ApplicationMutationAuditReceipt receipt,
        IReadOnlyCollection<AuditRecord> auditRecords)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentNullException.ThrowIfNull(auditRecords);

        if (!string.Equals(receipt.MutationManifestAlgorithm, _manifestHasher.Algorithm, StringComparison.Ordinal) ||
            auditRecords.Count != receipt.AuditRecordCount ||
            auditRecords.Any(record => !string.Equals(record.MutationBatchId, receipt.MutationBatchId, StringComparison.Ordinal)))
        {
            return false;
        }

        ApplicationMutationManifest manifest = _manifestBuilder.Build(auditRecords);
        string actualHash = _manifestHasher.ComputeHash(manifest);

        byte[] expectedBytes;
        byte[] actualBytes;

        try
        {
            expectedBytes = Convert.FromHexString(receipt.MutationManifestHash);
            actualBytes = Convert.FromHexString(actualHash);
        }
        catch (FormatException)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
