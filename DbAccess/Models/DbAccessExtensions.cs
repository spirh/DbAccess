using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using DbAccess.Contracts;
using DbAccess.Services;
using System.Reflection;
using DbAccess.Models;

namespace DbAccess;

/// <summary>
/// Extension methods for setting up DbAccess.
/// </summary>
public static class DbAccessExtensions
{
    /// <summary>
    /// Registers DbAccess services and repositories.
    /// </summary>
    public static IServiceCollection AddDbAccess(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrer DbAccessConfig fra konfigurasjon
        //services.Configure<DbAccessConfig>(configuration.GetSection("DbAccess"));

        // Registrer riktig `IDbQueryService`
        //services.AddScoped<IDbQueryService>(provider =>
        //{
        //    var dbConfig = provider.GetRequiredService<IOptions<DbAccessConfig>>().Value;
        //    return dbConfig.DatabaseType switch
        //    {
        //        "Postgres" => new PostgresQueryService(),
        //        "Mssql" => new MsSqlQueryService(),
        //        _ => throw new Exception("Invalid database type in configuration.")
        //    };
        //});

        // Registrer repository-typer
        services.AddScoped(typeof(IDbBasicRepository<>), typeof(BasicRepository<>));
        services.AddScoped(typeof(IDbExtendedRepository<,>), typeof(ExtendedRepository<,>));
        services.AddScoped(typeof(IDbCrossRepository<,,,>), typeof(CrossReferenceRepository<,,,>));

        // Dynamisk registrering av alle tjenester
        var serviceAssembly = Assembly.GetExecutingAssembly();
        var serviceTypes = serviceAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "DbAccess.Domain.Services");

        foreach (var type in serviceTypes)
        {
            var interfaceType = type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}");
            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, type);
            }
        }

        return services;
    }

    /// <summary>
    /// Applies any necessary database migrations or setup logic.
    /// </summary>
    public static void UseDbAccess(this IServiceProvider serviceProvider)
    {
        // Eksempel: Her kan vi sette opp database-migreringer, logging eller annen oppstartlogikk
        var dbConfig = serviceProvider.GetRequiredService<IOptions<DbAccessConfig>>().Value;
        Console.WriteLine($"Using database type: {dbConfig.DatabaseType}");
    }
}
