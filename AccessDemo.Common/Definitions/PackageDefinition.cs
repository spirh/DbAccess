using AccessDemo.Common.Models;
using DbAccess.Contracts;
using DbAccess.Helpers;

namespace AccessDemo.Common.Definitions;

public class PackageDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<Package>(def =>
        {
            def.SetHistory();
            def.SetTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.Name, length: 150);
            def.RegisterProperty(t => t.Description);
            def.RegisterProperty(t => t.AreaId);
            def.RegisterExtendedProperty<ExtPackage, Area>(t => t.AreaId, t => t.Id, t => t.Area);
            def.RegisterUniqueConstraint([t => t.Name, t => t.AreaId]);
        });
    }
}
