using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Volo.Abp.Mapperly.SampleClasses;
using Volo.Abp.ObjectExtending.TestObjects;

[Mapper]
public partial class MyEntityMapper : AbpMapperlyMapperBase<MyEntity, MyEntityDto>
{
    public override partial MyEntityDto Map(MyEntity source);

    public override partial void Map(MyEntity source, MyEntityDto destination);
}

[Mapper]
public partial class MyEnumMapper : AbpMapperlyMapperBase<MyEnum, MyEnumDto>
{
    public override partial MyEnumDto Map(MyEnum source);

    public override void Map(MyEnum source, MyEnumDto destination)
    {
        destination = Map(source);
    }
}

[Mapper]
[MapExtraProperties(IgnoredProperties = ["CityName"])]
public partial class ExtensibleTestPersonMapper : AbpMapperlyMapperBase<ExtensibleTestPerson, ExtensibleTestPersonDto>
{
    public override partial ExtensibleTestPersonDto Map(ExtensibleTestPerson source);

    public override partial void Map(ExtensibleTestPerson source, ExtensibleTestPersonDto destination);
}

[Mapper]
[MapExtraProperties(MapToRegularProperties = true)]
public partial class ExtensibleTestPersonWithRegularPropertiesDtoMapper : AbpMapperlyMapperBase<ExtensibleTestPerson, ExtensibleTestPersonWithRegularPropertiesDto>
{
    [MapperIgnoreTarget(nameof(ExtensibleTestPersonWithRegularPropertiesDto.Name))]
    [MapperIgnoreTarget(nameof(ExtensibleTestPersonWithRegularPropertiesDto.Age))]
    [MapperIgnoreTarget(nameof(ExtensibleTestPersonWithRegularPropertiesDto.IsActive))]
    public override partial ExtensibleTestPersonWithRegularPropertiesDto Map(ExtensibleTestPerson source);

    public override partial void Map(ExtensibleTestPerson source, ExtensibleTestPersonWithRegularPropertiesDto destination);
}
