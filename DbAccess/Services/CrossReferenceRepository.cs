using DbAccess.Contracts;
using DbAccess.Helpers;
using DbAccess.Models;
using Microsoft.Extensions.Options;
using System.Data;

namespace DbAccess.Services;

public abstract class CrossReferenceRepository<T, TExtended, TA, TB> : ExtendedRepository<T, TExtended>, IDbCrossRepository<T, TExtended, TA, TB> 
    where T : class, new()
{
    protected CrossReferenceRepository(IOptions<DbAccessConfig> options, IDbConnection connection, DbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }

    protected new DbCrossDefinition<T, TExtended, TA, TB> Definition { get; set; }

    public Task<IEnumerable<TA>> GetA(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TB>> GetB(Guid id)
    {
        throw new NotImplementedException();
    }
}
