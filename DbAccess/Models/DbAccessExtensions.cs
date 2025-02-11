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
