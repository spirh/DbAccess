using AccessDemo.Common.Models;
using DbAccess.Contracts;
using DbAccess.Helpers;

namespace AccessDemo.Common.Definitions;

public class ResourceDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<Resource>(def =>
        {
            def.SetHistory();
            def.SetTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.Name, length: 150);
            def.RegisterProperty(t => t.Description);
        });
    }
}
