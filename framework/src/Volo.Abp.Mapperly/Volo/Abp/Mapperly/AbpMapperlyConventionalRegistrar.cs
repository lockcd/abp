using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Mapperly;

public class AbpMapperlyConventionalRegistrar : DefaultConventionalRegistrar
{
    protected override bool IsConventionalRegistrationDisabled(Type type)
    {
        return !type.GetInterfaces().Any(x => x.IsGenericType && typeof(IAbpMapperly<,>) == x.GetGenericTypeDefinition()) ||
               base.IsConventionalRegistrationDisabled(type);
    }

    protected override List<Type> GetExposedServiceTypes(Type type)
    {
        var exposedServiceTypes = base.GetExposedServiceTypes(type);
        var mapperlyInterfaces = type.GetInterfaces().Where(x =>
            x.IsGenericType && (typeof(IAbpMapperly<,>) == x.GetGenericTypeDefinition() ||
                                typeof(IAbpReverseMapperly<,>) == x.GetGenericTypeDefinition()));
        return exposedServiceTypes
            .Union(mapperlyInterfaces)
            .Distinct()
            .ToList();
    }
}
