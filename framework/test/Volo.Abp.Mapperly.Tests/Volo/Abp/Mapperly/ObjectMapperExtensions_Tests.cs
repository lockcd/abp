using System;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Mapperly.SampleClasses;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Testing;
using Xunit;

namespace Volo.Abp.Mapperly;

public class ObjectMapperExtensions_Tests : AbpIntegratedTest<MapperlyTestModule>
{
    private readonly IObjectMapper _objectMapper;

    public ObjectMapperExtensions_Tests()
    {
        _objectMapper = ServiceProvider.GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public void Should_Map_Objects_With_AutoMap_Attributes()
    {
        var dto = _objectMapper.Map<MyEntity, MyEntityDto>(
            new MyEntity
            {
                Number = 42
            }
        );

        dto.As<MyEntityDto>().Number.ShouldBe(42);
    }
}
