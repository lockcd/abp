using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.AspNetCore.Mvc.Authorization;
using Volo.Abp.Security.Claims;
using Volo.Abp.Timing;
using Xunit;

namespace Volo.Abp.AspNetCore.Mvc.Timing;

public class AbpTimeZoneMiddleware_Tests : AspNetCoreMvcTestBase
{
    private readonly ICurrentTimezoneProvider _currentTimezoneProvider;
    private readonly FakeUserClaims _fakeRequiredService;

    public AbpTimeZoneMiddleware_Tests()
    {
        _currentTimezoneProvider = GetRequiredService<ICurrentTimezoneProvider>();
        _fakeRequiredService = GetRequiredService<FakeUserClaims>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Utc;
        });
    }

    [Fact]
    public async Task Should_Override_TimeZone_Setting_By_Request()
    {
        using (_currentTimezoneProvider.Change("UTC"))
        {
            var result = await Client.GetStringAsync("api/timing-test");
            result.ShouldBe("UTC");
        }

        // Query string
        using (_currentTimezoneProvider.Change("UTC"))
        {
            var result = await Client.GetStringAsync("api/timing-test?__timezone=Europe/Istanbul");
            result.ShouldBe("Europe/Istanbul");
        }

        // Header
        using (_currentTimezoneProvider.Change("UTC"))
        {
            Client.DefaultRequestHeaders.Add("__timezone", "Asia/Shanghai");
            var result = await Client.GetStringAsync("api/timing-test");
            result.ShouldBe("Asia/Shanghai");
        }

        // Form
        using (_currentTimezoneProvider.Change("UTC"))
        {
            Client.DefaultRequestHeaders.Remove("__timezone");
            var result = await Client.PostAsync("api/timing-test", new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("__timezone", "Europe/Germany")}));
            (await result.Content.ReadAsStringAsync()).ShouldBe("Europe/Germany");
        }

        // Cookie
        using (_currentTimezoneProvider.Change("UTC"))
        {
            Client.DefaultRequestHeaders.Remove("__timezone");
            Client.DefaultRequestHeaders.Add("Cookie", "__timezone=Europe/Istanbul");
            var result = await Client.GetStringAsync("api/timing-test");
            result.ShouldBe("Europe/Istanbul");
        }
    }

    [Fact]
    public async Task Should_Not_Override_TimeZone_Setting_By_Request_For_Authenticated_User()
    {
        _fakeRequiredService.Claims.AddRange(new[]
        {
            new Claim(AbpClaimTypes.UserId, AuthTestController.FakeUserId.ToString())
        });

        using (_currentTimezoneProvider.Change("Europe/Berlin"))
        {
            var result = await Client.GetStringAsync("api/timing-test");
            result.ShouldBe("UTC");
        }

        // Query string
        using (_currentTimezoneProvider.Change("Europe/Berlin"))
        {
            var result = await Client.GetStringAsync("api/timing-test?__timezone=Europe/Istanbul");
            result.ShouldBe("UTC");
        }

        // Header
        using (_currentTimezoneProvider.Change("Europe/Berlin"))
        {
            Client.DefaultRequestHeaders.Add("__timezone", "Asia/Shanghai");
            var result = await Client.GetStringAsync("api/timing-test");
            result.ShouldBe("UTC");
        }

        // Form
        using (_currentTimezoneProvider.Change("Europe/Berlin"))
        {
            Client.DefaultRequestHeaders.Remove("__timezone");
            var result = await Client.PostAsync("api/timing-test", new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("__timezone", "Europe/Germany")}));
            (await result.Content.ReadAsStringAsync()).ShouldBe("UTC");
        }

        // Cookie
        using (_currentTimezoneProvider.Change("Europe/Berlin"))
        {
            Client.DefaultRequestHeaders.Remove("__timezone");
            Client.DefaultRequestHeaders.Add("Cookie", "__timezone=Europe/Istanbul");
            var result = await Client.GetStringAsync("api/timing-test");
            result.ShouldBe("UTC");
        }
    }
}
