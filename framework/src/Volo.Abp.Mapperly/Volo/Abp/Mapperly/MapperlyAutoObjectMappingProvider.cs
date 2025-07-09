using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.ObjectExtending;
using Volo.Abp.ObjectMapping;

namespace Volo.Abp.Mapperly;

public class MapperlyAutoObjectMappingProvider<TContext> : MapperlyAutoObjectMappingProvider, IAutoObjectMappingProvider<TContext>
{
    public MapperlyAutoObjectMappingProvider(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}

public class MapperlyAutoObjectMappingProvider : IAutoObjectMappingProvider
{
    protected IServiceProvider ServiceProvider { get; }

    public MapperlyAutoObjectMappingProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual TDestination Map<TSource, TDestination>(object source)
    {
        var mapper = ServiceProvider.GetService<IAbpMapperly<TSource, TDestination>>();
        if (mapper != null)
        {
            mapper.BeforeMap((TSource)source);
            var destination = mapper.Map((TSource)source);
            TryMapExtraProperties(mapper, (TSource)source, destination, new ExtraPropertyDictionary());
            mapper.AfterMap((TSource)source, destination);
            return destination;
        }

        throw new AbpException($"No {nameof(IAbpMapperly<TSource, TDestination>)} mapper found for {typeof(TSource).FullName} to {typeof(TDestination).FullName}");
    }

    public virtual TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        var mapper = ServiceProvider.GetService<IAbpMapperly<TSource, TDestination>>();
        if (mapper != null)
        {
            mapper.BeforeMap(source);
            var destinationExtraProperties = GetExtraProperties(destination);
            mapper.Map(source, destination);
            TryMapExtraProperties(mapper, source, destination, destinationExtraProperties);
            mapper.AfterMap(source, destination);
            return destination;
        }

        throw new AbpException($"No {nameof(IAbpMapperly<TSource, TDestination>)} mapper found for {typeof(TSource).FullName} to {typeof(TDestination).FullName}");
    }

    protected virtual ExtraPropertyDictionary GetExtraProperties<TDestination>(TDestination destination)
    {
        var extraProperties = new ExtraPropertyDictionary();
        if (destination is not IHasExtraProperties hasExtraProperties)
        {
            return extraProperties;
        }

        foreach (var property in hasExtraProperties.ExtraProperties)
        {
            extraProperties.Add(property.Key, property.Value);
        }
        return extraProperties;
    }

    protected virtual void TryMapExtraProperties<TSource, TDestination>(IAbpMapperly<TSource, TDestination> mapper, TSource source, TDestination destination, ExtraPropertyDictionary destinationExtraProperty)
    {
        var mapToRegularPropertiesAttribute = mapper.GetType().GetSingleAttributeOrNull<MapExtraPropertiesAttribute>();
        if (mapToRegularPropertiesAttribute != null &&
            typeof(IHasExtraProperties).IsAssignableFrom(typeof(TDestination)) &&
            typeof(IHasExtraProperties).IsAssignableFrom(typeof(TSource)))
        {
            MapExtraPropertiesInvoker.Invoke<TSource, TDestination>(this,
                source!.As<IHasExtraProperties>(),
                destination!.As<IHasExtraProperties>(),
                destinationExtraProperty,
                mapToRegularPropertiesAttribute.DefinitionChecks,
                mapToRegularPropertiesAttribute.IgnoredProperties,
                mapToRegularPropertiesAttribute.MapToRegularProperties
            );
        }
    }

    protected virtual void MapExtraProperties<TSource, TDestination>(
        IHasExtraProperties source,
        IHasExtraProperties destination,
        ExtraPropertyDictionary destinationExtraProperty,
        MappingPropertyDefinitionChecks? definitionChecks = null,
        string[]? ignoredProperties = null,
        bool mapToRegularProperties = false)
        where TSource : IHasExtraProperties
        where TDestination : IHasExtraProperties
    {
        var result = destinationExtraProperty.IsNullOrEmpty()
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(destinationExtraProperty);

        if (source.ExtraProperties != null && destination.ExtraProperties != null)
        {
            ExtensibleObjectMapper
                .MapExtraPropertiesTo<TSource, TDestination>(
                    source.ExtraProperties,
                    result,
                    definitionChecks,
                    ignoredProperties
                );
        }

        ObjectHelper.TrySetProperty(destination, x => x.ExtraProperties, () => new ExtraPropertyDictionary(result));
        if (mapToRegularProperties)
        {
            destination.SetExtraPropertiesToRegularProperties();
        }
    }
}
