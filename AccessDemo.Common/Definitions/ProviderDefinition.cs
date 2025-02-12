using AccessDemo.Common.Models;
using DbAccess.Contracts;
using DbAccess.Helpers;

namespace AccessDemo.Common.Definitions;

public class ProviderDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<Provider>(def =>
        {
            def.SetHistory();
            def.SetTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.Name, length: 150);
        });
    }
}
