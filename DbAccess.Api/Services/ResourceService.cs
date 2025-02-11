using DbAccess.Api.Contracts;
using DbAccess.Api.Models;
using DbAccess.Contracts;
using DbAccess.Helpers;
using DbAccess.Models;
using DbAccess.Services;
using Microsoft.Extensions.Options;
using System.Data;

namespace DbAccess.Api.Services;

public class ResourceService : BasicRepository<Resource>, IResourceService
{
    public ResourceService(IOptions<DbAccessConfig> options, IDbConnection connection, DbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }

    public override void Define()
    {
        Definition.SetHistory();
        Definition.SetTranslation();
    }
}