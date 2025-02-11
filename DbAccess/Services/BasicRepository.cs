using System.Data;
using System.Reflection;
using Dapper;
using DbAccess.Contracts;
using DbAccess.Helpers;
using DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DbAccess.Services;
public abstract class BasicRepository<T> : IDbBasicRepository<T> 
    where T : class, new()
{
    private readonly DbAccessConfig config;
    private readonly IDbConnection connection;
    protected readonly DbConverter dbConverter;

    public BasicRepository(IOptions<DbAccessConfig> options, IDbConnection connection, DbConverter dbConverter)
    {
        config = options.Value;
        connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Define();
        this.dbConverter=dbConverter;
    }

    protected DbBasicDefinition<T> Definition { get; set; }
    public abstract void Define();

    public Task<IEnumerable<T>> Get()
    {
        throw new NotImplementedException();
    }
    public async Task<T> Get(Guid id)
    {
        throw new NotImplementedException();
    }
    public Task<IEnumerable<T>> Get(GenericFilterBuilder<T> filter)
    {
        throw new NotImplementedException();
    }
    public async Task<int> Insert(T entity, CancellationToken cancellationToken = default)
    {
        var param = GetEntityAsSqlParameter(entity);
        string query = "";// $"INSERT INTO {DbObjDef.BaseDbObject.GetPostgresDefinition(includeAlias: false)} ({InsertColumns([.. param.Keys])}) VALUES({InsertValues([.. param.Keys])})";
        return await ExecuteCommand(query, [.. param.Values], cancellationToken: cancellationToken);
    }
    public async Task Update(Guid id, T entity)
    {
        throw new NotImplementedException();
    }
    public async Task Delete(Guid id)
    {
        throw new NotImplementedException();
    }




    /*
     
    PRIVATE

    */

    /*HELPERS*/

    /// <summary>
    /// Generate filters based on object
    /// </summary>
    /// <param name="entity">Object</param>
    /// <returns></returns>
    protected Dictionary<string, NpgsqlParameter> GetEntityAsSqlParameter(object entity)
    {
        var parameters = new Dictionary<string, NpgsqlParameter>();
        foreach (PropertyInfo property in entity.GetType().GetProperties())
        {
            parameters.Add(property.Name, new NpgsqlParameter(property.Name, property.GetValue(entity) ?? DBNull.Value));
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
