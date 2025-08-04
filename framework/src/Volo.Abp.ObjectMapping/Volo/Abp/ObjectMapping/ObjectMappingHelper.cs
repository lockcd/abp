using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Volo.Abp.ObjectMapping;

public static class ObjectMappingHelper
{
    private static readonly ConcurrentDictionary<(Type, Type), (Type sourceArgumentType, Type destinationArgumentType, Type definitionGenericType)?> Cache = new();

    public static bool IsCollectionGenericType<TSource, TDestination>(
        out Type sourceArgumentType,
        out Type destinationArgumentType,
        out Type definitionGenericType)
    {
        var cached = Cache.GetOrAdd((typeof(TSource), typeof(TDestination)), _ => IsCollectionGenericTypeInternal<TSource, TDestination>());
        if (cached == null)
        {
            sourceArgumentType = destinationArgumentType = definitionGenericType = null!;
            return false;
        }

        (sourceArgumentType, destinationArgumentType, definitionGenericType) = cached.Value;
        return true;
    }

    private static (Type, Type, Type)? IsCollectionGenericTypeInternal<TSource, TDestination>()
    {
        Type sourceArgumentType = null!;
        Type destinationArgumentType = null!;
        Type definitionGenericType = null!;

        if ((!typeof(TSource).IsGenericType && !typeof(TSource).IsArray) ||
            (!typeof(TDestination).IsGenericType && !typeof(TDestination).IsArray))
        {
            return null;
        }

        var supportedCollectionTypes = new[]
        {
            typeof(IEnumerable<>),
            typeof(ICollection<>),
            typeof(Collection<>),
            typeof(IList<>),
            typeof(List<>)
        };

        if (typeof(TSource).IsGenericType && supportedCollectionTypes.Any(x => x == typeof(TSource).GetGenericTypeDefinition()))
        {
            sourceArgumentType = typeof(TSource).GenericTypeArguments[0];
        }

        if (typeof(TSource).IsArray)
        {
            sourceArgumentType = typeof(TSource).GetElementType()!;
        }

        if (sourceArgumentType == null)
        {
            return null;
        }

        definitionGenericType = typeof(List<>);
        if (typeof(TDestination).IsGenericType && supportedCollectionTypes.Any(x => x == typeof(TDestination).GetGenericTypeDefinition()))
        {
            destinationArgumentType = typeof(TDestination).GenericTypeArguments[0];

            if (typeof(TDestination).GetGenericTypeDefinition() == typeof(ICollection<>) ||
                typeof(TDestination).GetGenericTypeDefinition() == typeof(Collection<>))
            {
                definitionGenericType = typeof(Collection<>);
            }
        }

        if (typeof(TDestination).IsArray)
        {
            destinationArgumentType = typeof(TDestination).GetElementType()!;
            definitionGenericType = typeof(Array);
        }

        return (sourceArgumentType, destinationArgumentType, definitionGenericType);
    }
}
