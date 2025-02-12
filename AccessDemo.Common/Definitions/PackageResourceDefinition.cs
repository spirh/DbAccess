using AccessDemo.Common.Models;
using DbAccess.Contracts;
using DbAccess.Helpers;

namespace AccessDemo.Common.Definitions;

public class PackageResourceDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<PackageResource>(def =>
        {
            def.SetHistory();
            def.RegisterPrimaryKey([t => t.PackageId, t => t.ResourceId]);
            def.RegisterProperty(t => t.Id); // Remove?
            def.RegisterProperty(t => t.PackageId);
            def.RegisterProperty(t => t.ResourceId);
            def.RegisterAsCrossReferenceExtended<ExtPackageResource, Package, Resource>(
                defineA: (a => a.PackageId, a => a.Id, a => a.Package),
                defineB: (b => b.ResourceId, b => b.Id, b => b.Resource)
            );
            def.RegisterUniqueConstraint([t => t.PackageId, t => t.ResourceId]);
        });
    }
}