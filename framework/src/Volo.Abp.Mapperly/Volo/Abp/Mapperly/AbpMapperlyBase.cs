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

public abstract class AbpReverseMapperlyBase<TDestination, TSource> : AbpMapperlyBase<TSource, TDestination>, IAbpReverseMapperly<TDestination, TSource>
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
