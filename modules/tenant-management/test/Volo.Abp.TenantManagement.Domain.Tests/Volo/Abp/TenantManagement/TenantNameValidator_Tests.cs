using System;
using System.Threading.Tasks;
using Volo.Abp.Guids;
using Xunit;

namespace Volo.Abp.TenantManagement;

public class TenantNameValidator_Tests : AbpTenantManagementDomainTestBase
{
    private readonly ITenantNameValidator _tenantNameValidator;
    private readonly ITenantRepository _tenantRepository;
    public TenantNameValidator_Tests()
    {
        _tenantRepository = GetRequiredService<ITenantRepository>();
        _tenantNameValidator = GetRequiredService<ITenantNameValidator>();
    }

    [Fact]
    public async Task Should_Throw_If_Name_Is_Null()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _tenantNameValidator.ValidateAsync(null));
    }

    [Fact]
    public async Task Should_Throw_If_Duplicate_Name()
    {
        await Assert.ThrowsAsync<BusinessException>(() => _tenantNameValidator.ValidateAsync("VOLOSOFT"));
    }

    [Fact]
    public async Task Should_Not_Throw_For_Unique_Name()
    {
        await _tenantNameValidator.ValidateAsync("UNIQUE-TENANT-NAME");
    }
}
