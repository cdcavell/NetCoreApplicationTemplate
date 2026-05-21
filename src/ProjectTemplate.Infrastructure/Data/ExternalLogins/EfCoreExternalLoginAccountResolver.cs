using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.ExternalLogins;

/// <summary>
/// EF Core implementation of <see cref="IExternalLoginAccountResolver" />.
/// </summary>
public sealed class EfCoreExternalLoginAccountResolver(ApplicationDbContext dbContext)
    : IExternalLoginAccountResolver
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// Finds an external login account by the provider name and provider user ID.
    /// </summary>
    /// <param name="providerName">Provider name (e.g., "Google", "Facebook"). This value is case-insensitive and will be trimmed of whitespace.</param>
    /// <param name="providerUserId">Provider user ID (the unique identifier for the user from the external provider). This value is case-insensitive and will be trimmed of whitespace.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>Returns the matching <see cref="ExternalLoginAccount" /> if found; otherwise, returns null.</returns>
    public async Task<ExternalLoginAccount?> FindByProviderUserIdAsync(
        string providerName,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(providerName) || string.IsNullOrWhiteSpace(providerUserId))
        {
            return null;
        }

        string normalizedProviderName = providerName.Trim();
        string normalizedProviderUserId = providerUserId.Trim();

        return await _dbContext.ExternalLoginAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ProviderName == normalizedProviderName
                    && x.ProviderUserId == normalizedProviderUserId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Finds all external login accounts associated with a specific local user ID.
    /// </summary>
    /// <param name="localUserId">
    /// Local user ID (the unique identifier for the user in the local system). If this value is Guid.Empty, an empty list will be returned.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to cancel the operation if needed.
    /// </param>
    /// <returns>
    /// Returns a list of <see cref="ExternalLoginAccount" /> instances associated with the specified local user ID. If no accounts are found, an empty list is returned.
    /// </returns>
    public async Task<IReadOnlyList<ExternalLoginAccount>> FindByLocalUserIdAsync(
        Guid localUserId,
        CancellationToken cancellationToken = default)
    {
        return localUserId == Guid.Empty
            ? []
            : (IReadOnlyList<ExternalLoginAccount>)await _dbContext.ExternalLoginAccounts
            .AsNoTracking()
            .Where(x => x.LocalUserId == localUserId)
            .OrderBy(x => x.ProviderName)
            .ThenBy(x => x.ProviderUserId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
