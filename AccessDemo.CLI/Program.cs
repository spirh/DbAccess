// See https://aka.ms/new-console-template for more information
using AccessDemo.Common.Contracts;
using AccessDemo.Common.Services;
using DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;
using DbAccess.Helpers;
using AccessDemo.Common.Models;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static void Main()
    {
        DefinitionStore.RegisterAllDefinitions("AccessDemo.Common");
        var def = DefinitionStore.Definition<Package>();
        foreach(var col in def.Columns)
        {
            Console.WriteLine(col.Name);
        }

        var fakeConfig = Options.Create(new DbAccessConfig
        {
            ConnectionString = "Database=hhh;Host=localhost;Username=wigg;Password=jw8s0F4;Include Error Detail=true"
        });

        var fakeConnection = new NpgsqlConnection(fakeConfig.Value.ConnectionString);
        var packageService = new PackageService(fakeConfig, fakeConnection, DbConverter.Instance);

        Console.WriteLine("Hello!");

        RequestOptions opt = new RequestOptions();
        opt.Language = "eng";

        packageService.GetExtended().Wait();

        packageService.Get().Wait();
    }
}

/*
SELECT
Package.Id AS Id,coalesce(T_Package.Name, Package.Name) AS Name,coalesce(T_Package.Description, Package.Description) AS Description
,_Area.Id AS Area_Id,coalesce(T_Area.Name, _Area.Name) AS Area_Name
,_SuperArea.Id AS SuperArea_Id,coalesce(T_SuperArea.Name, _SuperArea.Name) AS SuperArea_Name

FROM dbo.Package AS Package

LEFT JOIN LATERAL (SELECT * FROM translation.Package AS T
WHERE T.Id = Package.Id AND T.Language = @Language) AS T_Package ON 1=1

INNER JOIN dbo.Area AS _Area ON Package.AreaId = _Area.Id
LEFT JOIN LATERAL (SELECT * FROM translation.Area AS T
WHERE T.Id = _Area.Id AND t.Language = @Language) AS T_Area on 1=1

INNER JOIN dbo.Area AS _SuperArea ON Package.SuperAreaId = _SuperArea.Id
LEFT JOIN LATERAL (SELECT * FROM translation.Area AS T
WHERE T.Id = _SuperArea.Id AND t.Language = @Language) AS T_SuperArea on 1=1
 
 */