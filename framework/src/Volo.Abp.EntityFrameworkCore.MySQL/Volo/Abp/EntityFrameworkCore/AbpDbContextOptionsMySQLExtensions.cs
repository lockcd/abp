using JetBrains.Annotations;
using System;

namespace Volo.Abp.EntityFrameworkCore;

public static class AbpDbContextOptionsMySQLExtensions
{
    [Obsolete("Use UsePomeloMySQL instead.")]
    public static void UseMySQL(
        [NotNull] this AbpDbContextOptions options,
        Action<Microsoft.EntityFrameworkCore.Infrastructure.MySqlDbContextOptionsBuilder>? mySQLOptionsAction = null)
    {
        options.UsePomeloMySQL(mySQLOptionsAction);
    }

    [Obsolete("Use UsePomeloMySQL instead.")]
    public static void UseMySQL<TDbContext>(
        [NotNull] this AbpDbContextOptions options,
        Action<Microsoft.EntityFrameworkCore.Infrastructure.MySqlDbContextOptionsBuilder>? mySQLOptionsAction = null)
        where TDbContext : AbpDbContext<TDbContext>
    {
        options.UsePomeloMySQL<TDbContext>(mySQLOptionsAction);
    }

    public static void UsePomeloMySQL(
        [NotNull] this AbpDbContextOptions options,
        Action<Microsoft.EntityFrameworkCore.Infrastructure.MySqlDbContextOptionsBuilder>? mySQLOptionsAction = null)
    {
        options.Configure(context =>
        {
            context.UsePomeloMySQL(mySQLOptionsAction);
        });
    }

    public static void UsePomeloMySQL<TDbContext>(
        [NotNull] this AbpDbContextOptions options,
        Action<Microsoft.EntityFrameworkCore.Infrastructure.MySqlDbContextOptionsBuilder>? mySQLOptionsAction = null)
        where TDbContext : AbpDbContext<TDbContext>
    {
        options.Configure<TDbContext>(context =>
        {
            context.UsePomeloMySQL(mySQLOptionsAction);
        });
    }

    public static void UseMySQLConnector(
        [NotNull] this AbpDbContextOptions options,
        Action<MySql.EntityFrameworkCore.Infrastructure.MySQLDbContextOptionsBuilder>? mySQLOptionsAction = null)
    {
        options.Configure(context =>
        {
            context.UseMySQLConnector(mySQLOptionsAction);
        });
    }

    public static void UseMySQLConnector<TDbContext>(
        [NotNull] this AbpDbContextOptions options,
        Action<MySql.EntityFrameworkCore.Infrastructure.MySQLDbContextOptionsBuilder>? mySQLOptionsAction = null)
        where TDbContext : AbpDbContext<TDbContext>
    {
        options.Configure<TDbContext>(context =>
        {
            context.UseMySQLConnector(mySQLOptionsAction);
        });
    }
}
