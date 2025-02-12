using DbAccess.Contracts;
using DbAccess.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace DbAccess.Helpers;

public static class DefinitionStore
{
    private static readonly ConcurrentDictionary<Type, DbDefinition> Store = new();

    public static void Define<T>(Action<DefinitionBuilder<T>> configure)
    {
        var builder = new DefinitionBuilder<T>();
        configure(builder);
        Store.AddOrUpdate(
            typeof(T),
            _ => builder.Build(), // Add
            (_, __) => builder.Build() // Update
        );
    }

    public static void Define<T>(DefinitionBuilder<T> builder)
    {
        Store.AddOrUpdate(
            typeof(T),
            _ => builder.Build(), // Add
            (_, __) => builder.Build() // Update
        );
    }

    public static void Define<T>(DbDefinition dbDefinition)
    {
        Store.AddOrUpdate(
            typeof(T),
            _ => dbDefinition, // Add
            (_, __) => dbDefinition // Update
        );
    }

    public static DbDefinition Definition<T>()
    {
        return Store.GetOrAdd(typeof(T), _ => new DefinitionBuilder<T>().Build());
    }
    public static DbDefinition? TryGetDefinition(Type type)
    {
        return Store.ContainsKey(type) ? Store[type] : null;
    }

    public static void RegisterAllDefinitions(string definitionNamespace = "")
    {
        List<IDbDefinition>? definitions;
        if (string.IsNullOrEmpty(definitionNamespace))
        {
            var executingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name!;

            definitions = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name!.StartsWith(executingAssemblyName)) // Sjekk mot hovedprosjektet
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IDbDefinition).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract
                    //&& t.Namespace?.StartsWith("MyProject.Definitions")
                    == true)
                .Select(t => (IDbDefinition)Activator.CreateInstance(t)!)
                .ToList();
        }
        else
        {
            var targetAssembly = Assembly.Load(definitionNamespace); // Bytt ut med riktig navn
            definitions = targetAssembly.GetTypes()
                .Where(t => typeof(IDbDefinition).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => (IDbDefinition)Activator.CreateInstance(t)!)
                .ToList();
        }

        foreach (var definition in definitions)
        {
            definition.Define();
        }
    }
}
