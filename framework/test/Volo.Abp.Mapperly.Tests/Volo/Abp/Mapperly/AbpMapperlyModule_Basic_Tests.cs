using System;
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
    public void Should_Map_Objects_With_Existing_Target_Object()
    {
        var dto = new MyEntityDto {Id = Guid.Empty, Number = 42};

        _objectMapper.Map<MyEntity, MyEntityDto>(new MyEntity { Id = Guid.NewGuid(), Number = 43 }, dto);

        dto.Number.ShouldBe(43);
        dto.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Should_Map_Enum()
    {
        var dto = _objectMapper.Map<MyEnum, MyEnumDto>(MyEnum.Value3);
        dto.ShouldBe(MyEnumDto.Value2); //Value2 is same as Value3
    }

    [Fact]
    public void Should_Throw_Exception_If_Mapper_Is_Not_Found()
    {
        var exception = Assert.Throws<AbpException>(() =>_objectMapper.Map<MyEntity, MyClassDto>(new MyEntity()));
        exception.Message.ShouldBe("No " +
                                   "Volo.Abp.Mapperly.IAbpMapperly<Volo.Abp.Mapperly.SampleClasses.MyEntity,Volo.Abp.Mapperly.MyClassDto> or " +
                                   "Volo.Abp.Mapperly.IAbpReverseMapperly<Volo.Abp.Mapperly.SampleClasses.MyEntity,Volo.Abp.Mapperly.MyClassDto>" +
                                   " was found");
    }
}
