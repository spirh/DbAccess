using DbAccess.Contracts;
using DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DbAccess.Services;

/// <inheritdoc/>
public abstract class CrossReferenceRepository<T, TExtended, TA, TB> : ExtendedRepository<T, TExtended>, IDbCrossRepository<T, TExtended, TA, TB> 
    where T : class, new()
    where TExtended : class, new()
{
    protected CrossReferenceRepository(IOptions<DbAccessConfig> options,  NpgsqlDataSource connection,  IDbConverter dbConverter) : base(options, connection, dbConverter) { }

    /// <inheritdoc/>
    public Task<IEnumerable<TA>> GetA(Guid id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<IEnumerable<TB>> GetB(Guid id)
    {
        throw new NotImplementedException();
    }
}
