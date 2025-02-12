using DbAccess.Contracts;
using DbAccess.Helpers;
using DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Text;

namespace DbAccess.Services;

public abstract class ExtendedRepository<T, TExtended> : BasicRepository<T>, IDbExtendedRepository<T, TExtended> 
    where T : class, new()
    where TExtended : class, new()
{
    protected ExtendedRepository(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter) { }

    public async Task<TExtended?> GetExtended(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var res = await GetExtended([new GenericFilter("Id", id)], options, cancellationToken);
        return res.FirstOrDefault();
    }
    public async Task<IEnumerable<TExtended>> GetExtended(RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await GetExtended(filters: [], options, cancellationToken);
    }
    public async Task<IEnumerable<TExtended>> GetExtended(GenericFilterBuilder<TExtended> filter, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await GetExtended(filters: filter, options, cancellationToken);
    }
    public async Task<IEnumerable<TExtended>> GetExtended(IEnumerable<GenericFilter> filters, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RequestOptions();
        filters ??= [];
        var cmd = GetCommand(options, filters);
        var param = PrepareParameters(filters, options);

        return await ExecuteExtended(cmd, param);
    }

    private async Task<IEnumerable<TExtended>> ExecuteExtended(string query, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var cmd = connection.CreateCommand(query);
            if(parameters != null && parameters.Any())
            {
                foreach(var p in parameters)
                {
                    cmd.Parameters.Add(new NpgsqlParameter(p.Key, p.Value));
                    //NpgsqlDbType? dbType; // Get from method...
                    //if (dbType.HasValue)
                    //{
                    //    cmd.Parameters.AddWithValue(p.Key, dbType.Value, p.Value);
                    //}
                    //else
                    //{
                    //    cmd.Parameters.Add(new NpgsqlParameter(p.Key, p.Value));
                    //}
                }
            }

            Console.WriteLine(query);
            return dbConverter.ConvertToObjects<TExtended>(await cmd.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(query);
            throw;
        }
    }


    private string GetCommand(RequestOptions? options = null, IEnumerable<GenericFilter>? filters = null)
    {
        options ??= new RequestOptions();
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));

        foreach (var relation in Definition.ForeignKeys)
        {
            sb.Append(',');
            sb.AppendLine(GenerateJoinPostgresColumns(relation, options));
        }

        sb.AppendLine("FROM " + GenerateSource(options));
        foreach (var j in Definition.ForeignKeys.Where(t => !t.IsList))
        {
            var joinStatement = GetJoinPostgresStatement(j, options);
            sb.AppendLine(joinStatement);
        }

        sb.AppendLine(GenerateStatementFromFilters(Definition.BaseType.Name, filters));

        var query = sb.ToString();

        Console.WriteLine(query);

        query = AddPagingToQuery(query, options);

        return query;
    }

    private string GetJoinPostgresFilterString(ForeignKeyDefinition join)
    {
        if (join.Filters == null || join.Filters.Count == 0)
        {
            return "";
        }

        string result = string.Empty;

        foreach (var filter in join.Filters)
        {
            result += $" AND {join.Base.Name}.{filter.PropertyName} = _{join.ExtendedProperty}.{filter.Value}";
        }

        return result;
    }

    /// <summary>
    /// Generate Postgres join statement
    /// </summary>
    /// <param name="join">Join</param>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    public string GetJoinPostgresStatement(ForeignKeyDefinition join, RequestOptions options)
    {
        var joinDef = DefinitionStore.TryGetDefinition(join.Ref) ?? throw new InvalidOperationException();
        bool useHistory = options.AsOf.HasValue;

        var sb = new StringBuilder();

        sb.Append($"{(join.IsOptional ? "LEFT OUTER" : "INNER")} JOIN {GetPostgresDefinition(joinDef, includeAlias: false, useHistory: useHistory)} AS _{join.ExtendedProperty} ON {join.Base.Name}.{join.BaseProperty} = _{join.ExtendedProperty}.{join.RefProperty} {GetJoinPostgresFilterString(join)}");

        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        if (useTranslation && joinDef.HasTranslation)
        {
            sb.AppendLine();
            sb.Append($"LEFT JOIN LATERAL (SELECT * FROM {GetPostgresDefinition(joinDef, useTranslation: true, includeAlias: false, useHistory: useHistory)} AS T ");
            sb.Append($"WHERE T.Id = _{join.ExtendedProperty}.Id AND t.Language = @Language) AS T_{join.ExtendedProperty} ON 1=1");
        }

        return sb.ToString();
    }

    public string GenerateJoinPostgresColumns(ForeignKeyDefinition join, RequestOptions options)
    {
        var joinDef = DefinitionStore.TryGetDefinition(join.Ref);
        if (joinDef == null)
        {
            return string.Empty;
        }

        if (!join.IsList)
        {
            bool useTranslation = !string.IsNullOrEmpty(options.Language);
            var columns = new List<string>();
            foreach (var p in joinDef.Columns.Select(t=>t.Property))
            {
                if (joinDef.HasTranslation && useTranslation && p.PropertyType == typeof(string))
                {
                    columns.Add($"coalesce(T_{join.ExtendedProperty}.{p.Name}, _{join.ExtendedProperty}.{p.Name}) AS {join.ExtendedProperty}_{p.Name}");
                    //columns.Add($"coalesce({join.Alias}{join.JoinObj.TranslationDbObject.Alias}.{p.Name},_{join.Alias}.{p.Name}) AS {join.Alias}_{p.Name}");
                }
                else
                {
                    columns.Add($"_{join.ExtendedProperty}.{p.Name} AS {join.ExtendedProperty}_{p.Name}");
                }
            }

            return string.Join(',', columns);
        }
        else
        {
            return $"COALESCE((SELECT JSON_AGG(ROW_TO_JSON({join.ExtendedProperty})) FROM {GetPostgresDefinition(joinDef, includeAlias: false)} AS {join.ExtendedProperty} WHERE {join.ExtendedProperty}.{join.RefProperty} = {join.Base.Name}.{join.BaseProperty}), '[]') AS {join.ExtendedProperty}";
        }
    }


}
