using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.TenantManagement;

public class TenantManager : DomainService, ITenantManager
{
    public ITenantNameValidator TenantNameValidator { get; }
    protected ITenantNormalizer TenantNormalizer { get; }
    protected ILocalEventBus LocalEventBus { get; }

    public TenantManager(
        ITenantNameValidator tenantNameValidator,
        ITenantNormalizer tenantNormalizer,
        ILocalEventBus localEventBus)
    {
        TenantNameValidator = tenantNameValidator;
        TenantNormalizer = tenantNormalizer;
        LocalEventBus = localEventBus;
    }

    public virtual async Task<Tenant> CreateAsync(string name)
    {
        Check.NotNull(name, nameof(name));

        var normalizedName = TenantNormalizer.NormalizeName(name);
        await TenantNameValidator.ValidateAsync(normalizedName);
        return new Tenant(GuidGenerator.Create(), name, normalizedName);
    }

    public virtual async Task ChangeNameAsync(Tenant tenant, string name)
    {
        Check.NotNull(tenant, nameof(tenant));
        Check.NotNull(name, nameof(name));

        var normalizedName = TenantNormalizer.NormalizeName(name);

        await TenantNameValidator.ValidateAsync(normalizedName, tenant.Id);
        await LocalEventBus.PublishAsync(new TenantChangedEvent(tenant.Id, tenant.NormalizedName));
        tenant.SetName(name);
        tenant.SetNormalizedName(normalizedName);
    }
}
