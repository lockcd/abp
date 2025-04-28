using System;
using System.Threading.Tasks;

namespace Volo.Abp.TenantManagement;

public interface ITenantNameValidator
{
    Task ValidateAsync(string normalizedTenantName, Guid? expectedTenantId = null);
}