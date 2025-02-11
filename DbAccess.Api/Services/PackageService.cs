using DbAccess.Api.Contracts;
using DbAccess.Api.Models;
using DbAccess.Helpers;
using DbAccess.Models;
using DbAccess.Services;
using Microsoft.Extensions.Options;
using System.Data;

namespace DbAccess.Api.Services;
public class PackageService : ExtendedRepository<Package, ExtPackage>, IPackageService
{
    public PackageService(IOptions<DbAccessConfig> options, IDbConnection connection, DbConverter dbConverter) : base(options, connection, dbConverter) { }

    public override void Define()
    {
        Definition.SetHistory();
        Definition.SetTranslation();

        Definition.RegisterProperty(t => t.Description);
        Definition.RegisterProperty(t => t.Name, length: 150);

        Definition.RegisterExtendedProperty<Area>(t => t.AreaId, t => t.Id, t => t.Area);

        Definition.RegisterUniqueConstraint([t => t.Name, t => t.AreaId]);
    }
}
