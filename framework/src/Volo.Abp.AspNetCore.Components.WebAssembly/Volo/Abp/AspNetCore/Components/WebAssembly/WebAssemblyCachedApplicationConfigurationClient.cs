﻿using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Volo.Abp.AspNetCore.Components.Web.Security;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations.ClientProxies;
using Volo.Abp.AspNetCore.Mvc.Client;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace Volo.Abp.AspNetCore.Components.WebAssembly;

public class WebAssemblyCachedApplicationConfigurationClient : ICachedApplicationConfigurationClient, ITransientDependency
{
    protected AbpApplicationConfigurationClientProxy ApplicationConfigurationClientProxy { get; }

    protected AbpApplicationLocalizationClientProxy ApplicationLocalizationClientProxy { get; }

    protected ApplicationConfigurationCache Cache { get; }

    protected ICurrentTenantAccessor CurrentTenantAccessor { get; }

    protected ICurrentTimezoneProvider CurrentTimezoneProvider { get; }

    protected ApplicationConfigurationChangedService ApplicationConfigurationChangedService { get; }

    protected IJSRuntime JSRuntime { get; }

    protected IClock Clock { get; }

    public WebAssemblyCachedApplicationConfigurationClient(
        AbpApplicationConfigurationClientProxy applicationConfigurationClientProxy,
        ApplicationConfigurationCache cache,
        ICurrentTenantAccessor currentTenantAccessor,
        ICurrentTimezoneProvider currentTimezoneProvider,
        AbpApplicationLocalizationClientProxy applicationLocalizationClientProxy,
        ApplicationConfigurationChangedService applicationConfigurationChangedService,
        IJSRuntime jsRuntime,
        IClock clock)
    {
        ApplicationConfigurationClientProxy = applicationConfigurationClientProxy;
        Cache = cache;
        CurrentTenantAccessor = currentTenantAccessor;
        CurrentTimezoneProvider = currentTimezoneProvider;
        ApplicationLocalizationClientProxy = applicationLocalizationClientProxy;
        ApplicationConfigurationChangedService = applicationConfigurationChangedService;
        JSRuntime = jsRuntime;
        Clock = clock;
    }

    public virtual async Task InitializeAsync()
    {
        var configurationDto = await ApplicationConfigurationClientProxy.GetAsync(
            new ApplicationConfigurationRequestOptions {
                IncludeLocalizationResources = false
            }
        );

        var localizationDto = await ApplicationLocalizationClientProxy.GetAsync(
            new ApplicationLocalizationRequestDto {
                CultureName = configurationDto.Localization.CurrentCulture.Name,
                OnlyDynamics = true
            }
        );

        configurationDto.Localization.Resources = localizationDto.Resources;

        Cache.Set(configurationDto);

        if (!configurationDto.CurrentUser.IsAuthenticated)
        {
            await JSRuntime.InvokeVoidAsync("abp.utils.removeOidcUser");
        }

        ApplicationConfigurationChangedService.NotifyChanged();

        CurrentTenantAccessor.Current = new BasicTenantInfo(
            configurationDto.CurrentTenant.Id,
            configurationDto.CurrentTenant.Name
        );

        if (Clock.SupportsMultipleTimezone)
        {
            CurrentTimezoneProvider.TimeZone = !configurationDto.Timing.TimeZone.Iana.TimeZoneName.IsNullOrWhiteSpace()
                ? configurationDto.Timing.TimeZone.Iana.TimeZoneName
                : await JSRuntime.InvokeAsync<string>("abp.clock.getBrowserTimeZone");

            await JSRuntime.InvokeAsync<string>("abp.clock.setBrowserTimeZoneToCookie");
        }
    }

    public virtual Task<ApplicationConfigurationDto> GetAsync()
    {
        return Task.FromResult(GetConfigurationByChecking());
    }

    public virtual ApplicationConfigurationDto Get()
    {
        return GetConfigurationByChecking();
    }

    private ApplicationConfigurationDto GetConfigurationByChecking()
    {
        var configuration = Cache.Get();
        if (configuration == null)
        {
            throw new AbpException(
                $"{nameof(WebAssemblyCachedApplicationConfigurationClient)} should be initialized before using it.");
        }

        return configuration;
    }
}
