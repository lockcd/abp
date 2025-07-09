using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Mapperly.SampleClasses;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Testing;
using Xunit;

namespace Volo.Abp.Mapperly;

public class AbpMapperlyModule_Basic_Tests : AbpIntegratedTest<MapperlyTestModule>
{
    private readonly IObjectMapper _objectMapper;

    public AbpMapperlyModule_Basic_Tests()
    {
        _objectMapper = ServiceProvider.GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public void Should_Replace_IAutoObjectMappingProvider()
    {
        Assert.True(ServiceProvider.GetRequiredService<IAutoObjectMappingProvider>() is MapperlyAutoObjectMappingProvider);
    }

    [Fact]
    public void Should_Map_Objects_With_AutoMap_Attributes()
    {
        var dto = _objectMapper.Map<MyEntity, MyEntityDto>(new MyEntity { Number = 42 });
        dto.Number.ShouldBe(42);
    }

    [Fact]
    public void Should_Map_Enum()
    {
        var dto = _objectMapper.Map<MyEnum, MyEnumDto>(MyEnum.Value3);
        dto.ShouldBe(MyEnumDto.Value2); //Value2 is same as Value3
    }
}
