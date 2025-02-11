using System.Data;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dapper;
using DbAccess.Contracts;
using DbAccess.Helpers;
using DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;

namespace DbAccess.Services;
public abstract class BasicRepository<T> : IDbBasicRepository<T> 
    where T : class, new()
{
    private readonly DbAccessConfig config;
    private readonly IDbConnection connection;
    protected readonly DbConverter dbConverter;

    public BasicRepository(IOptions<DbAccessConfig> options, IDbConnection connection, DbConverter dbConverter)
    {
        this.config = options.Value;
        this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        this.dbConverter = dbConverter;
        Define();
    }

    protected DbBasicDefinition<T> Definition { get; set; }
    public abstract void Define();
    public GenericFilterBuilder<T> CreateFilterBuilder<T>() { return new GenericFilterBuilder<T>(); }





    public async Task<IEnumerable<T>> Get(RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await Get(filters: new List<GenericFilter>(), options: options, cancellationToken: cancellationToken);
    }
    public async Task<T?> Get(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var res = await Get(new List<GenericFilter>() { new GenericFilter("id", id) }, options, cancellationToken: cancellationToken);
        return res.FirstOrDefault();
    }
    public async Task<IEnumerable<T>> Get(GenericFilterBuilder<T> filterBuilder, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await Get(filters: filterBuilder, options: options, cancellationToken: cancellationToken);
    }
    public async Task<IEnumerable<T>> Get(IEnumerable<GenericFilter> filters, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RequestOptions();
        filters ??= [];
        var cmd = GetCommand(options, filters);
        var param = PrepareParameters(filters, options);
        return await ExecuteQuery(cmd, param, cancellationToken: cancellationToken);
    }





    #region Write




    /// <inheritdoc/>
    public async Task<int> Ingest(List<T> data, CancellationToken cancellationToken = default)
    {
        using var conn = (NpgsqlConnection)GetConnection();
        if (conn.State != ConnectionState.Open) 
        {
            conn.Open();
        }

        var dt = new DataTable();
        var dataAdapter = new NpgsqlDataAdapter($"SELECT * FROM {GetPostgresDefinition(includeAlias: false)} LIMIT 10", conn);
        dataAdapter.Fill(dt);
        dt.Clear();

        var columns = new Dictionary<string, (NpgsqlDbType Type, PropertyInfo Property)>();
        
        foreach (DataColumn c in dt.Columns)
        {
            if (!Definition.Columns.Exists(t => t.Name.Equals(c.ColumnName, StringComparison.CurrentCultureIgnoreCase)))
            {
                continue;
            }

            columns.Add(c.ColumnName, (GetPostgresType(c.DataType), Definition.Columns.First(t => t.Name.Equals(c.ColumnName, StringComparison.CurrentCultureIgnoreCase)).Property));
        }

        using var writer = await conn.BeginBinaryImportAsync($"COPY {GetPostgresDefinition(includeAlias: false)} ({string.Join(',', columns.Keys)}) FROM STDIN (FORMAT BINARY)", cancellationToken: cancellationToken);
        writer.Timeout = new TimeSpan(0, 10, 0);
        int batchCompleted = 0;
        int batchSize = 10000;
        int completed = 0;
        foreach (var d in data)
        {
            writer.StartRow();
            foreach (var c in columns)
            {
                try
                {
                    writer.Write(c.Value.Property.GetValue(d), c.Value.Type);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write data in column '{c.Key}' for '{Definition.EntityType.Name}'. Trying to write null. " + ex.Message);
                    try
                    {
                        writer.WriteNull();
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to write null in column '{c.Key}' for '{Definition.EntityType.Name}'.");
                        throw;
                    }
                }
            }

            completed++;
            if (completed == batchSize)
            {
                batchCompleted++;
                completed = 0;
                Console.WriteLine($"Ingested {(batchCompleted * batchSize) + completed}");
            }
        }

        Console.WriteLine($"Ingested {(batchCompleted * batchSize) + completed}");
        writer.Complete();

        return (batchCompleted * batchSize) + completed;
    }

    /// <inheritdoc/>
    public async Task<int> Insert(T entity, CancellationToken cancellationToken = default)
    {
        
        var param = GetObjectAsSqlParameter(entity);
        string query = $"INSERT INTO {GetPostgresDefinition(includeAlias: false)} ({InsertColumns([.. param.Keys])}) VALUES({InsertValues([.. param.Keys])})";
        return await ExecuteCommand(query, [.. param.Values], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Upsert(Guid id, T entity, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        var param = GetObjectAsSqlParameter(entity);
        sb.AppendLine($"INSERT INTO {GetPostgresDefinition(includeAlias: false)} ({InsertColumns([.. param.Keys])}) VALUES({InsertValues([.. param.Keys])})");
        sb.AppendLine(" ON CONFLICT (id) DO ");
        sb.AppendLine($"UPDATE SET {UpdateSetStatement([.. param.Keys])}");
        param.Add("_id", new NpgsqlParameter("_id", id));
        return await ExecuteCommand(sb.ToString(), [.. param.Values], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default)
    {
        var param = GetObjectAsSqlParameter(entity);
        string query = $"UPDATE {GetPostgresDefinition(includeAlias: false)} SET {UpdateSetStatement([.. param.Keys])} WHERE Id = @_id";
        param.Add("_id", new NpgsqlParameter("_id", id));
        return await ExecuteCommand(query, [.. param.Values], cancellationToken: cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<int> Update(Guid id, List<GenericParameter> parameters, CancellationToken cancellationToken = default)
    {
        string query = $"UPDATE {GetPostgresDefinition(includeAlias: false)} SET {UpdateSetStatement(parameters.Select(t => t.Key).ToList())} WHERE id = @_id";
        var queryParameters = new List<NpgsqlParameter>();
        if (parameters != null && parameters.Count > 0)
        {
            foreach (var p in parameters)
            {
                queryParameters.Add(new NpgsqlParameter(p.Key, p.Value));
            }
        }
        queryParameters.Add(new NpgsqlParameter("_id", id));
        return await ExecuteCommand(query, queryParameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        string query = $"DELETE FROM {GetPostgresDefinition(includeAlias: false)} WHERE id = @_id";
        return await ExecuteCommand(query, [new NpgsqlParameter("_id", id)], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> CreateTranslation(T obj, string language, CancellationToken cancellationToken = default)
    {
        if (!Definition.HasTranslation)
        {
            return 0;
        }

        var param = GetTranslationObjectAsSqlParameter(obj);
        param.Add("Language", new NpgsqlParameter("Language", language));
        var query = $"INSERT INTO {GetPostgresDefinition(includeAlias: false, useTranslation: true)} ({InsertColumns([.. param.Keys])}) VALUES({InsertValues([.. param.Keys])})";
        return await ExecuteCommand(query, [.. param.Values], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> UpdateTranslation(Guid id, T obj, string language, CancellationToken cancellationToken = default)
    {
        if (!Definition.HasTranslation)
        {
            return 0;
        }

        var param = GetTranslationObjectAsSqlParameter(obj);
        string query = $"UPDATE {GetPostgresDefinition(includeAlias: false, useTranslation: true)} SET {UpdateSetStatement([.. param.Keys])} WHERE Id = @_id AND Language = @_language";
        param.Add("_id", new NpgsqlParameter("_id", id));
        param.Add("Language", new NpgsqlParameter("_language", language));
        return await ExecuteCommand(query, [.. param.Values], cancellationToken: cancellationToken);
    }
    #endregion

    /*HELPERS*/

    private string GetCommand(RequestOptions? options = null, IEnumerable<GenericFilter>? filters = null)
    {
        options ??= new RequestOptions();
        var sb = new StringBuilder();

        if (options != null)
        {
            if (options.AsOf.HasValue)
            {
                // FORMAT : 2025-01-22 12:03:50.240333 +00:00
                sb.AppendLine($"set session x.asof = '{options.AsOf.Value.ToUniversalTime()}';");
            }
        }

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));
        sb.AppendLine("FROM " + GenerateSource(options));
        sb.AppendLine(GenerateStatementFromFilters(Definition.EntityType.Name, filters));

        var query = sb.ToString();
        query = AddPagingToQuery(query, options);

        return query;
    }


    /// <summary>
    /// Generate columns for query
    /// </summary>
    /// <param name="dbObjDef">Object definition</param>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    protected string GenerateColumns(RequestOptions options)
    {
        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        if (!Definition.HasTranslation)
        {
            useTranslation = false;
        }

        var columns = new List<string>();
        foreach (var p in Definition.Columns.Select(t=>t.Property))
        {
            if (useTranslation && Definition.HasTranslation && p.PropertyType == typeof(string))
            {
                columns.Add($"coalesce(T_{Definition.EntityType.Name}.{p.Name},{Definition.EntityType.Name}.{p.Name}) AS {p.Name}");
            }
            else
            {
                columns.Add($"{Definition.EntityType.Name}.{p.Name} AS {p.Name}");
            }
        }

        if (options.UsePaging)
        {
            string orderBy = string.IsNullOrEmpty(options.OrderBy) ? "Id" : options.OrderBy;
            Definition.Columns.Exists(t=>t.Name.Equals(orderBy, StringComparison.CurrentCultureIgnoreCase));
            columns.Add($"ROW_NUMBER() OVER (ORDER BY {Definition.EntityType.Name}.{orderBy}) AS _rownum");
        }

        return string.Join(',', columns);
    }

    protected string GenerateStatementFromFilters(string tableAlias, IEnumerable<GenericFilter>? filters)
    {
        if (filters == null)
        {
            return string.Empty;
        }

        var conditions = new List<string>();

        foreach (var filter in filters)
        {
            string condition = filter.Comparer switch
            {
                FilterComparer.Equals => $"{tableAlias}.{filter.PropertyName} = @{filter.PropertyName}",
                FilterComparer.NotEqual => $"{tableAlias}.{filter.PropertyName} <> @{filter.PropertyName}",
                FilterComparer.GreaterThan => $"{tableAlias}.{filter.PropertyName} > @{filter.PropertyName}",
                FilterComparer.GreaterThanOrEqual => $"{tableAlias}.{filter.PropertyName} >= @{filter.PropertyName}",
                FilterComparer.LessThan => $"{tableAlias}.{filter.PropertyName} < @{filter.PropertyName}",
                FilterComparer.LessThanOrEqual => $"{tableAlias}.{filter.PropertyName} <= @{filter.PropertyName}",
                FilterComparer.StartsWith => $"{tableAlias}.{filter.PropertyName} ILIKE @{filter.PropertyName}",
                FilterComparer.EndsWith => $"{tableAlias}.{filter.PropertyName} ILIKE @{filter.PropertyName}",
                FilterComparer.Contains => $"{tableAlias}.{filter.PropertyName} ILIKE @{filter.PropertyName}",
                _ => throw new NotSupportedException($"Comparer '{filter.Comparer}' is not supported.")
            };

            conditions.Add(condition);
        }

        return conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;
    }


    protected string AddPagingToQuery(string query, RequestOptions options)
    {
        if (!options.UsePaging)
        {
            return query;
        }

        var sb = new StringBuilder();

        sb.AppendLine("WITH pagedresult AS (");
        sb.AppendLine(query);
        sb.AppendLine(")");
        sb.AppendLine("SELECT *");
        sb.AppendLine("FROM pagedresult, (SELECT MAX(pagedresult._rownum) AS totalitems FROM pagedresult) AS pageinfo");
        sb.AppendLine($"ORDER BY _rownum OFFSET {options.PageSize * (options.PageNumber - 1)} ROWS FETCH NEXT {options.PageSize} ROWS ONLY");

        return sb.ToString();
    }

    protected string GenerateSource(RequestOptions options)
    {
        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        bool useHistory = options.AsOf.HasValue;

        if (!Definition.HasTranslation)
        {
            // TODO: And Language system default
            useTranslation = false;
        }

        if (useTranslation && Definition.HasTranslation)
        {
            return $"""
            {GetPostgresDefinition(includeAlias: true, useHistory: useHistory)}
            LEFT JOIN LATERAL (SELECT * FROM {GetPostgresDefinition(includeAlias: false, useTranslation: true, useHistory: useHistory)} AS T 
            WHERE T.Id = {Definition.EntityType.Name}.Id AND T.Language = @Language) AS T_{Definition.EntityType.Name} ON 1=1
            """;
        }
        else
        {
            return GetPostgresDefinition(useHistory: useHistory);
        }
    }


    protected Dictionary<string, object> PrepareParameters(IEnumerable<GenericFilter>? filters, RequestOptions options)
    {
        var parameters = new Dictionary<string, object>();

        if (filters != null)
        {
            foreach (var filter in filters)
            {
                object value = filter.Comparer switch
                {
                    FilterComparer.StartsWith => $"{filter.Value}%",
                    FilterComparer.EndsWith => $"%{filter.Value}",
                    FilterComparer.Contains => $"%{filter.Value}%",
                    _ => filter.Value
                };

                parameters.Add(filter.PropertyName, value);
            }
        }

        if (options.Language != null)
        {
            parameters.Add("Language", options.Language);
        }

        if (options.AsOf.HasValue)
        {
            parameters.Add("_AsOf", options.AsOf.Value);
        }

        return parameters;
    }




    public static NpgsqlDbType GetPostgresType(Type type)
    {
        if (type == typeof(string))
        {
            return NpgsqlDbType.Text;
        }

        if (type == typeof(int))
        {
            return NpgsqlDbType.Integer;
        }

        if (type == typeof(DateTimeOffset))
        {
            return NpgsqlDbType.TimestampTz;
        }

        if (type == typeof(Guid))
        {
            return NpgsqlDbType.Uuid;
        }

        if (type == typeof(bool))
        {
            return NpgsqlDbType.Boolean;
        }

        Console.WriteLine($"Typeconverter not found for '{type.Name}'");

        return NpgsqlDbType.Text;
    }

    public string GetPostgresDefinition(bool includeAlias = true, bool useHistory = false, bool useTranslation = false)
    {
        // If Definition.Plantform == "Mssql" => Qualify names [..]

        string res = "";
        if (useHistory)
        {
            if (useTranslation)
            {
                res = $"{Definition.TranslationHistorySchema}.{Definition.EntityType.Name}";
            }
            else
            {
                res = $"{Definition.BaseHistorySchema}.{Definition.EntityType.Name}";
            }
        }
        else
        {
            if (useTranslation)
            {
                res = $"{Definition.TranslationSchema}.{Definition.EntityType.Name}";
            }
            else
            {
                res = $"{Definition.BaseSchema}.{Definition.EntityType.Name}";
            }
        }

        if (includeAlias)
        {
            res += $" AS {Definition.EntityType.Name}";
        }

        return res;
    }

    /// <summary>
    /// Generate filters based on object
    /// </summary>
    /// <param name="obj">Object</param>
    /// <returns></returns>
    protected Dictionary<string, NpgsqlParameter> GetObjectAsSqlParameter(object obj)
    {
        var parameters = new Dictionary<string, NpgsqlParameter>();
        foreach (PropertyInfo property in obj.GetType().GetProperties())
        {
            parameters.Add(property.Name, new NpgsqlParameter(property.Name, property.GetValue(obj) ?? DBNull.Value));
        }

        return parameters;
    }

    /// <summary>
    /// Get translation filters
    /// Ignores non-string values
    /// </summary>
    /// <param name="obj">Translated object</param>
    /// <returns></returns>
    private Dictionary<string, NpgsqlParameter> GetTranslationObjectAsSqlParameter(object obj)
    {
        var parameters = new Dictionary<string, NpgsqlParameter>();
        foreach (PropertyInfo property in obj.GetType().GetProperties())
        {
            if (property.PropertyType == typeof(string) || property.Name == "Id")
            {
                parameters.Add(property.Name, new NpgsqlParameter(property.Name, property.GetValue(obj) ?? DBNull.Value));
            }
        }

        return parameters;
    }

    private string UpdateSetStatement(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"{t} = @{t}").ToList());
    }

    private string InsertColumns(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"{t}").ToList());
    }

    private string InsertValues(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"@{t}").ToList());
    }


    /*EXECUTE*/

    private IDbConnection GetConnection()
    {
        if (connection != null)
        {
            return connection;
        }
        else if (!string.IsNullOrWhiteSpace(config.ConnectionString))
        {
            return new NpgsqlConnection(config.ConnectionString);
        }
        else
        {
            throw new InvalidOperationException("Neither an IDbConnection nor a valid connection string was provided.");
        }
    }

    private async Task<int> ExecuteCommand(string query, List<NpgsqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            using var conn = (NpgsqlConnection)GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.Parameters.AddRange(parameters.ToArray());
            cmd.CommandText = query;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            return await cmd.ExecuteNonQueryAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(query);
            foreach (NpgsqlParameter param in parameters)
            {
                Console.WriteLine($"{param.ParameterName}:{param.Value}");
            }

            throw;
        }
    }

    private async Task<IEnumerable<T>> ExecuteQuery(string query, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var conn = (NpgsqlConnection)GetConnection();
            var cmd = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);
            await using var reader = await conn.ExecuteReaderAsync(cmd);
            return dbConverter.ConvertToObjects<T>(reader);
        }
        catch (Exception ex)
        {
            Console.WriteLine(query);
            Console.WriteLine(ex.Message);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    Console.WriteLine($"{param.Key}:{param.Value}");
                }
            }

            throw;
        }
    }
}
