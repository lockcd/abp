using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Mapperly;

public abstract class AbpMapperlyMapperBase<TSource, TDestination> : IAbpMapperlyMapper<TSource, TDestination>, ITransientDependency
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

public abstract class AbpReverseMapperlyMapperBase<TDestination, TSource> : AbpMapperlyMapperBase<TSource, TDestination>, IAbpReverseMapperlyMapper<TDestination, TSource>
{
    public abstract TSource ReverseMap(TDestination destination);

    public abstract void ReverseMap(TDestination destination, TSource source);

    public virtual void BeforeReverseMap(TDestination destination)
    {
    }

    public virtual void AfterReverseMap(TDestination destination, TSource source)
    {
    }
}
