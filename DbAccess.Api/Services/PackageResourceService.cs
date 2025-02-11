using DbAccess.Api.Contracts;
using DbAccess.Api.Models;
using DbAccess.Contracts;
using DbAccess.Helpers;
using DbAccess.Models;
using DbAccess.Services;
using Microsoft.Extensions.Options;
using System.Data;

namespace DbAccess.Api.Services;

public class PackageResourceService : CrossReferenceRepository<PackageResource, ExtPackageResource, Package, Resource>, IPackageResourceService
{
    public PackageResourceService(IOptions<DbAccessConfig> options, IDbConnection connection, DbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }

    public override void Define()
    {
        Definition.SetHistory();

        Definition.RegisterPrimaryKey([t => t.Id]);

        Definition.RegisterProperty(t => t.Id);
        Definition.RegisterProperty(t => t.PackageId);
        Definition.RegisterProperty(t => t.ResourceId);

        Definition.RegisterExtendedProperty<Package>(t => t.PackageId, t => t.Id, t => t.Package, cascadeDelete: true);
        Definition.RegisterExtendedProperty<Resource>(t => t.ResourceId, t => t.Id, t => t.Resource, cascadeDelete: true);

        Definition.RegisterAsCrossRefrence(t => t.PackageId, t => t.Id, t => t.ResourceId, t => t.Id);
    }
}