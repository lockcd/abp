# Migrating from AutoMapper to Mapperly

## Introduction

The AutoMapper library is no longer free for commercial use. See [this article](https://www.jimmybogard.com/automapper-and-mediatr-going-commercial/) for more details.

ABP framework provides the AutoMapper and Mapperly integration. If you have a project that uses AutoMapper and don't have any license for AutoMapper, you can migrate to Mapperly by following the steps below.

## Migration Steps

Please open your project with the IDE(`Visual Studio`, `VS Code` or `JetBrains Rider`), then perform a global search and replace.

1. Replace `Volo.Abp.AutoMapper` to `Volo.Abp.Mapperly` in all `*.csproj` files.

2. Replace `using Volo.Abp.AutoMapper;` to `using Volo.Abp.Mapperly;` in all `*.cs` files.

3. Replace `AbpAutoMapperModule` with `AbpMapperlyModule` in all `*.cs` files.

4. Replace `AddAutoMapperObjectMapper` to `AddMapperlyObjectMapper` in all `*.cs` files.

5. Remove `Configure<AbpAutoMapperOptions>` code section.

6. Check the AutoMapper's `Profile` class to add all new Mapperly mapping classes.

## Example

Here is an AutoMapper's `Profile` class:

```csharp
public class ExampleAutoMapper : Profile
{
    public AbpIdentityApplicationModuleAutoMapperProfile()
    {
        CreateMap<IdentityUser, IdentityUserDto>()
            .MapExtraProperties()
            .Ignore(x => x.IsLockedOut)
            .Ignore(x => x.SupportTwoFactor)
            .Ignore(x => x.RoleNames);

        CreateMap<IdentityUserClaim, IdentityUserClaimDto>();

        CreateMap<OrganizationUnit, OrganizationUnitDto>()
            .MapExtraProperties();

		CreateMap<OrganizationUnitRole, OrganizationUnitRoleDto>()
			.ReverseMap();

        CreateMap<IdentityRole, OrganizationUnitRoleDto>()
            .ForMember(dest => dest.RoleId, src => src.MapFrom(r => r.Id));

        CreateMap<IdentityUser, IdentityUserExportDto>()
            .ForMember(dest => dest.Active, src => src.MapFrom(r => r.IsActive ? "Yes" : "No"))
            .ForMember(dest => dest.EmailConfirmed, src => src.MapFrom(r => r.EmailConfirmed ? "Yes" : "No"))
            .ForMember(dest => dest.TwoFactorEnabled, src => src.MapFrom(r => r.TwoFactorEnabled ? "Yes" : "No"))
            .ForMember(dest => dest.AccountLookout, src => src.MapFrom(r => r.LockoutEnd != null && r.LockoutEnd > DateTime.UtcNow ? "Yes" : "No"))
            .Ignore(x => x.Roles);
    }
}
```

And the Mapperly mapping class:

```csharp
[Mapper]
[MapExtraProperties]
public partial class IdentityUserToIdentityUserDtoMapper : MapperBase<IdentityUser, IdentityUserDto>
{
    [MapperIgnoreTarget(nameof(IdentityUserDto.IsLockedOut))]
    [MapperIgnoreTarget(nameof(IdentityUserDto.SupportTwoFactor))]
    [MapperIgnoreTarget(nameof(IdentityUserDto.RoleNames))]
    public override partial IdentityUserDto Map(IdentityUser source);

    [MapperIgnoreTarget(nameof(IdentityUserDto.IsLockedOut))]
    [MapperIgnoreTarget(nameof(IdentityUserDto.SupportTwoFactor))]
    [MapperIgnoreTarget(nameof(IdentityUserDto.RoleNames))]
    public override partial void Map(IdentityUser source, IdentityUserDto destination);
}

[Mapper]
public partial class IdentityUserClaimToIdentityUserClaimDtoMapper : MapperBase<IdentityUserClaim, IdentityUserClaimDto>
{
    public override partial IdentityUserClaimDto Map(IdentityUserClaim source);

    public override partial void Map(IdentityUserClaim source, IdentityUserClaimDto destination);
}

[Mapper]
[MapExtraProperties]
public partial class OrganizationUnitToOrganizationUnitDtoMapper : MapperBase<OrganizationUnit, OrganizationUnitDto>
{
    public override partial OrganizationUnitDto Map(OrganizationUnit source);
    public override partial void Map(OrganizationUnit source, OrganizationUnitDto destination);
}

[Mapper]
public partial class OrganizationUnitRoleToOrganizationUnitRoleDtoMapper : TwoWayMapperBase<OrganizationUnitRole, OrganizationUnitRoleDto>
{
    public override partial OrganizationUnitRoleDto Map(OrganizationUnitRole source);
    public override partial void Map(OrganizationUnitRole source, OrganizationUnitRoleDto destination);

    public override partial OrganizationUnitRole ReverseMap(OrganizationUnitRoleDto destination);
    public override partial void ReverseMap(OrganizationUnitRoleDto destination, OrganizationUnitRole source);
}

[Mapper]
[MapExtraProperties]
public partial class OrganizationUnitToOrganizationUnitWithDetailsDtoMapper : MapperBase<OrganizationUnit, OrganizationUnitWithDetailsDto>
{
    [MapperIgnoreTarget(nameof(OrganizationUnitWithDetailsDto.Roles))]
    [MapperIgnoreTarget(nameof(OrganizationUnitWithDetailsDto.UserCount))]
    public override partial OrganizationUnitWithDetailsDto Map(OrganizationUnit source);

    [MapperIgnoreTarget(nameof(OrganizationUnitWithDetailsDto.Roles))]
    [MapperIgnoreTarget(nameof(OrganizationUnitWithDetailsDto.UserCount))]
    public override partial void Map(OrganizationUnit source, OrganizationUnitWithDetailsDto destination);
}

[Mapper]
public partial class IdentityRoleToOrganizationUnitRoleDtoMapper : MapperBase<IdentityRole, OrganizationUnitRoleDto>
{
    [MapProperty(nameof(IdentityRole.Id), nameof(OrganizationUnitRoleDto.RoleId))]
    public override partial OrganizationUnitRoleDto Map(IdentityRole source);

    [MapProperty(nameof(IdentityRole.Id), nameof(OrganizationUnitRoleDto.RoleId))]
    public override partial void Map(IdentityRole source, OrganizationUnitRoleDto destination);
}

[Mapper]
public partial class IdentityUserToIdentityUserExportDtoMapper : MapperBase<IdentityUser, IdentityUserExportDto>
{
    [MapperIgnoreTarget(nameof(IdentityUserExportDto.Roles))]
    public override partial IdentityUserExportDto Map(IdentityUser source);

    [MapperIgnoreTarget(nameof(IdentityUserExportDto.Roles))]
    public override partial void Map(IdentityUser source, IdentityUserExportDto destination);

    public override void AfterMap(IdentityUser source, IdentityUserExportDto destination)
    {
        destination.Active = source.IsActive ? "Yes" : "No";
        destination.EmailConfirmed = source.EmailConfirmed ? "Yes" : "No";
        destination.TwoFactorEnabled = source.TwoFactorEnabled ? "Yes" : "No";
        destination.AccountLookout = source.LockoutEnd != null && source.LockoutEnd > DateTime.UtcNow ? "Yes" : "No";
    }
}
```

## Mapperly Mapping Class

You need to create a new Mapperly mapping class for each source and destination type.

The `Mapper` attribute is used to mark the class as a Mapperly mapping class.

The `MapperIgnoreTarget` attribute is used to replace the `Ignore` method.

The `MapExtraProperties` attribute is used to replace the `MapExtraProperties` method.

The `TwoWayMapperBase` class is used to replace the `ReverseMap` method.

The `AfterMap` method is used to perform actions after the mapping.

### Dependency Injection in Mapper Class

All the Mapperly mapping classes will be added to the DI container. If you want to inject a service in your Mapper class, You just need to add the service to the constructor of the Mapper class.

```csharp
public partial class IdentityUserToIdentityUserDtoMapper : MapperBase<IdentityUser, IdentityUserDto>
{
    public IdentityUserToIdentityUserDtoMapper(MyService myService)
    {
        _myService = myService;
    }

    public override partial IdentityUserDto Map(IdentityUser source);
    public override partial void Map(IdentityUser source, IdentityUserDto destination);

    public override void AfterMap(IdentityUser source, IdentityUserDto destination)
    {
        destination.MyProperty = _myService.GetMyProperty(source.MyProperty);
    }
}
```

## Mapperly Documentation

Please refer to the [Mapperly documentation](https://mapperly.riok.app/docs/intro/) for more details.

Key points:

- [Mapperly Configuration](https://mapperly.riok.app/docs/configuration/mapper/)
- [Mapperly Enums](https://mapperly.riok.app/docs/configuration/enum/)
- [Mapperly Flattening and unflattening](https://mapperly.riok.app/docs/configuration/flattening/)
