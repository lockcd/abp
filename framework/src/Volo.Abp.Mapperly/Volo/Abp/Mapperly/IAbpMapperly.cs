namespace Volo.Abp.Mapperly;

public interface IAbpMapperly<in TSource, TDestination>
{
    TDestination Map(TSource source);

    void Map(TSource source, TDestination destination);

    void BeforeMap(TSource source);

    void AfterMap(TSource source, TDestination destination);
}
