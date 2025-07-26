using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;

namespace Volo.Abp.EntityFrameworkCore;

public static class AbpDbContextConfigurationContextMySQLExtensions
{
    /// <summary>
    /// This extension method is configuring the use Pomelo.EntityFrameworkCore.MySql as the database provider.
    /// </summary>
    [Obsolete("Use UsePomeloMySQL instead.")]
    public static DbContextOptionsBuilder UseMySQL(
       [NotNull] this AbpDbContextConfigurationContext context,
       Action<Microsoft.EntityFrameworkCore.Infrastructure.MySqlDbContextOptionsBuilder>? mySQLOptionsAction = null)
    {
        return context.UsePomeloMySQL(mySQLOptionsAction);
    }

    /// <summary>
    /// This extension method is configuring the use Pomelo.EntityFrameworkCore.MySql as the database provider.
    /// </summary>
    public static DbContextOptionsBuilder UsePomeloMySQL(
        [NotNull] this AbpDbContextConfigurationContext context,
        Action<Microsoft.EntityFrameworkCore.Infrastructure.MySqlDbContextOptionsBuilder>? mySQLOptionsAction = null)
    {
        if (context.ExistingConnection != null)
        {
            return context.DbContextOptions.UseMySql(context.ExistingConnection,
                ServerVersion.AutoDetect(context.ConnectionString), optionsBuilder =>
                {
                    optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    mySQLOptionsAction?.Invoke(optionsBuilder);
                });
        }
        else
        {
            return context.DbContextOptions.UseMySql(context.ConnectionString,
                ServerVersion.AutoDetect(context.ConnectionString), optionsBuilder =>
                {
                    optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    mySQLOptionsAction?.Invoke(optionsBuilder);
                });
        }
    }

    /// <summary>
    /// This extension method is configuring the use MySql.EntityFrameworkCore  as the database provider.
    /// </summary>
    public static DbContextOptionsBuilder UseMySQLConnector(
        [NotNull] this AbpDbContextConfigurationContext context,
        Action<MySql.EntityFrameworkCore.Infrastructure.MySQLDbContextOptionsBuilder>? mySQLOptionsAction = null)
    {
        if (context.ExistingConnection != null)
        {
            return context.DbContextOptions.UseMySQL(context.ExistingConnection, mySQLOptionsAction);
        }
        else
        {
            return context.DbContextOptions.UseMySQL(context.ConnectionString, mySQLOptionsAction);
        }
    }
}
