using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.AspNetCore.MultiTenancy;

namespace Microsoft.AspNetCore.Builder;

public static class AbpAspNetCoreMultiTenancyApplicationBuilderExtensions
{
    private const string AuthenticationMiddlewareSetKey = "__AuthenticationMiddlewareSet";


    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        var authenticationMiddlewareSet = app.Properties.TryGetValue(AuthenticationMiddlewareSetKey, out var value) && value is true;
        if (!authenticationMiddlewareSet)
        {
            var logger = app.ApplicationServices.GetService<ILogger<MultiTenancyMiddleware>>();
            logger?.LogWarning("MultiTenancyMiddleware is being registered before the authentication middleware. " +
                               "This may lead to incorrect tenant resolution. " +
                               "Ensure that app.UseAuthentication() is called before app.UseMultiTenancy().");
        }
        return app.UseMiddleware<MultiTenancyMiddleware>();
    }
}
