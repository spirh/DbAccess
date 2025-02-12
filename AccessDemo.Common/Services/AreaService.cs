using AccessDemo.Common.Contracts;
using AccessDemo.Common.Models;
using DbAccess.Contracts;
using DbAccess.Models;
using DbAccess.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace AccessDemo.Common.Services;

public class AreaService : ExtendedRepository<Area, ExtArea>, IAreaService
{
    public AreaService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter) 
    { }
}
