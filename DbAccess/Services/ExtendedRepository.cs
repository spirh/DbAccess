using DbAccess.Contracts;
using DbAccess.Helpers;
using DbAccess.Models;
using Microsoft.Extensions.Options;
using System.Data;

namespace DbAccess.Services;

public abstract class ExtendedRepository<T, TExtended> : BasicRepository<T>, IDbExtendedRepository<T, TExtended> 
    where T : class, new()
{
    protected ExtendedRepository(IOptions<DbAccessConfig> options, IDbConnection connection, DbConverter dbConverter) : base(options, connection, dbConverter) { }

    protected new DbExtendedDefinition<T, TExtended> Definition { get; set; }

    public Task<TExtended> GetExtended(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TExtended>> GetExtended()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TExtended>> GetExtended(GenericFilterBuilder<TExtended> filter)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TExtended>> GetExtended(IEnumerable<GenericFilter> filters)
    {
        throw new NotImplementedException();
    }
}
