using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Volo.Abp.Data;
using Volo.Abp.ObjectExtending;

namespace Volo.Abp.Mapperly;

public static class MapExtraPropertiesInvoker
{
    private delegate void MapMethodDelegate(
        object targetInstance,
        IHasExtraProperties source,
        IHasExtraProperties destination,
        ExtraPropertyDictionary destinationExtraProperty,
        MappingPropertyDefinitionChecks? definitionChecks,
        string[]? ignoredProperties,
        bool mapToRegularProperties);

    private readonly static ConcurrentDictionary<(Type, Type), MapMethodDelegate> Cache = new();

    private readonly static MethodInfo MethodDefinition = typeof(MapperlyAutoObjectMappingProvider).GetMethod("MapExtraProperties", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public static void Invoke<TSource, TDestination>(
        object targetInstance,
        IHasExtraProperties source,
        IHasExtraProperties destination,
        ExtraPropertyDictionary destinationExtraProperty,
        MappingPropertyDefinitionChecks? definitionChecks = null,
        string[]? ignoredProperties = null,
        bool mapToRegularProperties = false)
    {
        var mapExtraProperties = Cache.GetOrAdd((typeof(TSource), typeof(TDestination)), static key =>
        {
            Check.NotNull(MethodDefinition, nameof(MethodDefinition));

            var genericMethod = MethodDefinition.MakeGenericMethod(key.Item1, key.Item2);

            var targetParam = Expression.Parameter(typeof(object), "target");
            var sourceParam = Expression.Parameter(typeof(IHasExtraProperties), "source");
            var destParam = Expression.Parameter(typeof(IHasExtraProperties), "destination");
            var checksParam = Expression.Parameter(typeof(MappingPropertyDefinitionChecks?), "checks");
            var destinationExtraPropertyParam = Expression.Parameter(typeof(ExtraPropertyDictionary), "destinationExtraProperty");
            var ignoredParam = Expression.Parameter(typeof(string[]), "ignored");
            var mapFlagParam = Expression.Parameter(typeof(bool), "mapFlag");

            var instanceCast = Expression.Convert(targetParam, typeof(MapperlyAutoObjectMappingProvider));

            var call = Expression.Call(
                instanceCast,
                genericMethod,
                sourceParam,
                destParam,
                destinationExtraPropertyParam,
                checksParam,
                ignoredParam,
                mapFlagParam
            );

            var lambda = Expression.Lambda<MapMethodDelegate>(
                call,
                targetParam, sourceParam, destParam, destinationExtraPropertyParam, checksParam, ignoredParam, mapFlagParam
            );

            return lambda.Compile();
        });

        mapExtraProperties(targetInstance, source, destination, destinationExtraProperty, definitionChecks, ignoredProperties, mapToRegularProperties);
    }
}
