using Microsoft.Extensions.DependencyInjection;
using Riok.Mapperly.Abstractions;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Testing;
using Xunit;

namespace Volo.Abp.Mapperly;

public class MyReverseClass
{
    public string Id { get; set; }

    public string Name { get; set; }
}

public class MyReverseClassDto
{
    public string Id { get; set; }

    public string Name { get; set; }
}

[Mapper]
public partial class MyReverseClassMapper : AbpReverseMapperlyBase<MyReverseClassDto, MyReverseClass>
{
    public override partial MyReverseClassDto Map(MyReverseClass source);

    public override partial void Map(MyReverseClass source, MyReverseClassDto destination);

    public override partial MyReverseClass ReverseMap(MyReverseClassDto destination);

    public override partial void ReverseMap(MyReverseClassDto destination, MyReverseClass source);

    public override void BeforeReverseMap(MyReverseClassDto destination)
    {
        destination.Name = "BeforeReverseMap " + destination.Name;
    }

    public override void AfterReverseMap(MyReverseClassDto destination, MyReverseClass source)
    {
        source.Name = destination.Name + " AfterReverseMap";
    }
}

public class AbpReverseMapperly_Tests : AbpIntegratedTest<MapperlyTestModule>
{
    private readonly IObjectMapper _objectMapper;

    public AbpReverseMapperly_Tests()
    {
        _objectMapper = ServiceProvider.GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public void Map_Test()
    {
        var myClass = new MyReverseClass { Id = "1", Name = "Test" };
        var myClassDto = _objectMapper.Map<MyReverseClass, MyReverseClassDto>(myClass);
        myClassDto.Name.ShouldBe("Test");
    }

    [Fact]
    public void ReverseMap_Test()
    {
        var myClassDto = new MyReverseClassDto { Id = "1", Name = "Test" };

        var myClass = _objectMapper.Map<MyReverseClassDto, MyReverseClass>(myClassDto);

        myClass.Name.ShouldBe("BeforeReverseMap Test AfterReverseMap");
    }
}
