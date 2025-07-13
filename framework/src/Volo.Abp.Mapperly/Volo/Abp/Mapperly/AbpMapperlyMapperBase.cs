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

public abstract class AbpReverseMapperlyMapperBase<TSource, TDestination> : AbpMapperlyMapperBase<TSource, TDestination>, IAbpReverseMapperlyMapper<TSource, TDestination>
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
