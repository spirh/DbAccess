﻿using AccessDemo.Common.Models;
using DbAccess.Contracts;
using DbAccess.Helpers;

namespace AccessDemo.Common.Definitions;

public class AreaDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<Area>(def =>
        {
            def.SetHistory();
            def.SetTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.Name, length: 150);
            def.RegisterUniqueConstraint([t => t.Name]);
        });
    }
}
