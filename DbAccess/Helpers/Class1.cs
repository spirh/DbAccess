using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DbAccess.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbServices(this IServiceCollection services, string assemblyName)
    {
        var targetAssembly = Assembly.Load(assemblyName);

        var serviceTypes = targetAssembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => i.Name.EndsWith("Service"))); // Henter alle som slutter på "Service"

        foreach (var serviceType in serviceTypes)
        {
            var interfaceType = serviceType.GetInterfaces().FirstOrDefault(i => i.Name == $"I{serviceType.Name}");
            if (interfaceType != null)
            {
                Console.WriteLine($"Registering {serviceType.Name} as {interfaceType.Name}");
                services.AddScoped(interfaceType, serviceType);
            }
        }

        return services;
    }
}
