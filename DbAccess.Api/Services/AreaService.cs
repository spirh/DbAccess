using DbAccess.Api.Contracts;
using DbAccess.Api.Models;
using DbAccess.Helpers;
using DbAccess.Models;
using DbAccess.Services;
using Microsoft.Extensions.Options;
using System.Data;

namespace DbAccess.Api.Services;

public class AreaService : BasicRepository<Area>, IAreaService
{
    public AreaService(IOptions<DbAccessConfig> options, IDbConnection connection, DbConverter dbConverter) : base(options, connection, dbConverter) { }

    public override void Define()
    {
        Definition.SetHistory();
        Definition.SetTranslation();

        Definition.RegisterPrimaryKey([t => t.Id]);

        Definition.RegisterProperty(t => t.Id);
        Definition.RegisterProperty(t => t.Name, length: 150);

        //RegisterRelation<Package>(t => t.Id, t => t.AreaId, t => t.Packages);

        Definition.RegisterUniqueConstraint([t => t.Name]);
    }
}
