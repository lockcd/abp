using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace Volo.Abp.TenantManagement;

public class DefaultTenantNameValidator : ITenantNameValidator, ITransientDependency
{
    protected ITenantRepository TenantRepository { get; }

    public DefaultTenantNameValidator(ITenantRepository tenantRepository)
    {
        TenantRepository = tenantRepository;
    }

    public virtual async Task ValidateAsync(string normalizedTenantName, Guid? expectedTenantId = null)
    {
        Check.NotNullOrWhiteSpace(normalizedTenantName, nameof(normalizedTenantName));

        var existingTenant = await TenantRepository.FindByNameAsync(normalizedTenantName);
        if (existingTenant != null && existingTenant.Id != expectedTenantId)
        {
            throw new BusinessException("Volo.Abp.TenantManagement:DuplicateTenantName")
                .WithData("Name", normalizedTenantName);
        }
    }
}