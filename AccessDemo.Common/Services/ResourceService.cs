using AccessDemo.Common.Contracts;
using AccessDemo.Common.Models;
using DbAccess.Contracts;
using DbAccess.Models;
using DbAccess.Services;
using Microsoft.Extensions.Options;
using System.Data;

namespace AccessDemo.Common.Services;

public class ResourceService : ExtendedRepository<Resource, ExtResource>, IResourceService
{
    public ResourceService(IOptions<DbAccessConfig> options, IDbConnection connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
