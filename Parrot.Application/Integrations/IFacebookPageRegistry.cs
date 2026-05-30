namespace Parrot.Application.Integrations;

/// <summary>
/// Resolves which tenant owns a given Facebook Page ID.
/// Each SaaS customer links their own Facebook Page; this registry maps
/// Page ID → tenant so webhook events can be routed to the correct account.
/// </summary>
public interface IFacebookPageRegistry
{
    /// <summary>
    /// Returns the tenant identifier for the given Facebook Page ID,
    /// or null if the page has not been linked by any customer.
    /// </summary>
    Task<string?> ResolveTenantAsync(string pageId, CancellationToken cancellationToken = default);
}
