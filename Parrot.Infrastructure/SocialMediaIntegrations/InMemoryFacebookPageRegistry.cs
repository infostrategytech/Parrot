using System.Collections.Concurrent;
using Parrot.Application.Integrations;

namespace Parrot.Infrastructure.SocialMediaIntegrations;

// Thread-safe in-memory registry mapping Facebook Page IDs → tenant IDs.
// Populated at runtime when a customer completes the Facebook page linking flow.
// Replace with a database-backed implementation once persistent tenant storage is wired up.
public sealed class InMemoryFacebookPageRegistry : IFacebookPageRegistry
{
    private readonly ConcurrentDictionary<string, string> _pageToTenant = new(StringComparer.Ordinal);

    public void Register(string pageId, string tenantId) =>
        _pageToTenant[pageId] = tenantId;

    public void Unregister(string pageId) =>
        _pageToTenant.TryRemove(pageId, out _);

    public Task<string?> ResolveTenantAsync(string pageId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_pageToTenant.TryGetValue(pageId, out string? tenantId) ? tenantId : null);
}
