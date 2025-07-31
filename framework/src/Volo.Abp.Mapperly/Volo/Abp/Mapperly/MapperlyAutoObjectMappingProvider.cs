using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.ObjectExtending;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Reflection;

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
        var mapper = ServiceProvider.GetService<IAbpMapperlyMapper<TSource, TDestination>>();
        if (mapper != null)
        {
            mapper.BeforeMap((TSource)source);
            var destination = mapper.Map((TSource)source);
            TryMapExtraProperties(mapper.GetType().GetSingleAttributeOrNull<MapExtraPropertiesAttribute>(), (TSource)source, destination, new ExtraPropertyDictionary());
            mapper.AfterMap((TSource)source, destination);
            return destination;
        }

        var reverseMapper = ServiceProvider.GetService<IAbpReverseMapperlyMapper<TDestination, TSource>>();
        if (reverseMapper != null)
        {
            reverseMapper.BeforeReverseMap((TSource)source);
            var destination = reverseMapper.ReverseMap((TSource)source);
            TryMapExtraProperties(reverseMapper.GetType().GetSingleAttributeOrNull<MapExtraPropertiesAttribute>(), (TSource)source, destination, GetExtraProperties(destination));
            reverseMapper.AfterReverseMap((TSource)source, destination);
            return destination;
        }

        throw new AbpException($"No {TypeHelper.GetFullNameHandlingNullableAndGenerics(typeof(IAbpMapperlyMapper<TSource, TDestination>))} or" +
                               $" {TypeHelper.GetFullNameHandlingNullableAndGenerics(typeof(IAbpReverseMapperlyMapper<TSource, TDestination>))} was found");
    }

    public virtual TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        var mapper = ServiceProvider.GetService<IAbpMapperlyMapper<TSource, TDestination>>();
        if (mapper != null)
        {
            mapper.BeforeMap(source);
            var destinationExtraProperties = GetExtraProperties(destination);
            mapper.Map(source, destination);
            TryMapExtraProperties(mapper.GetType().GetSingleAttributeOrNull<MapExtraPropertiesAttribute>(), source, destination, destinationExtraProperties);
            mapper.AfterMap(source, destination);
            return destination;
        }

        var reverseMapper = ServiceProvider.GetService<IAbpReverseMapperlyMapper<TDestination, TSource>>();
        if (reverseMapper != null)
        {
            reverseMapper.BeforeReverseMap(source);
            var destinationExtraProperties = GetExtraProperties(destination);
            reverseMapper.ReverseMap(source, destination);
            TryMapExtraProperties(reverseMapper.GetType().GetSingleAttributeOrNull<MapExtraPropertiesAttribute>(), source, destination, destinationExtraProperties);
            reverseMapper.AfterReverseMap(source, destination);
            return destination;
        }

        throw new AbpException($"No {TypeHelper.GetFullNameHandlingNullableAndGenerics(typeof(IAbpMapperlyMapper<TSource, TDestination>))} or" +
                               $" {TypeHelper.GetFullNameHandlingNullableAndGenerics(typeof(IAbpReverseMapperlyMapper<TSource, TDestination>))} was found");
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

    protected virtual void TryMapExtraProperties<TSource, TDestination>(MapExtraPropertiesAttribute? mapExtraPropertiesAttribute, TSource source, TDestination destination, ExtraPropertyDictionary destinationExtraProperty)
    {
        if (mapExtraPropertiesAttribute != null &&
            typeof(IHasExtraProperties).IsAssignableFrom(typeof(TDestination)) &&
            typeof(IHasExtraProperties).IsAssignableFrom(typeof(TSource)))
        {
            MapExtraProperties<TSource, TDestination>(
                source!.As<IHasExtraProperties>(),
                destination!.As<IHasExtraProperties>(),
                destinationExtraProperty,
                mapExtraPropertiesAttribute.DefinitionChecks,
                mapExtraPropertiesAttribute.IgnoredProperties,
                mapExtraPropertiesAttribute.MapToRegularProperties
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
    {
        var result = destinationExtraProperty.IsNullOrEmpty()
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(destinationExtraProperty);

        if (source.ExtraProperties != null && destination.ExtraProperties != null)
        {
            ExtensibleObjectMapper
                .MapExtraPropertiesTo(
                    typeof(TSource),
                    typeof(TDestination),
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
