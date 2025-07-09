using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Mapperly;

public abstract class AbpMapperlyBase<TSource, TDestination> : IAbpMapperly<TSource, TDestination>, ITransientDependency
{
    public abstract TDestination Map(TSource source);

    public abstract void Map(TSource source, TDestination destination);

    public virtual void BeforeMap(TSource source)
    {

    }
    public virtual void AfterMap(TSource source, TDestination destination)
    {

    }
}
