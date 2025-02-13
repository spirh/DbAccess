using AccessDemo.Api;
using AccessDemo.Common.Models;
using DbAccess.Contracts;
using DbAccess.Helpers;
using DbAccess.Models;
using DbAccess.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DbAccessConfig>(builder.Configuration.GetRequiredSection("DbAccessConfig"));
DefinitionStore.RegisterAllDefinitions("AccessDemo.Common");
builder.Services.AddSingleton<IDbConverter, DbConverter>();
builder.Services.AddSingleton(NpgsqlDataSource.Create("Database=newtests;Host=localhost;Username=wigg;Password=jw8s0F4;Include Error Detail=true"));
builder.Services.AddDbServices("AccessDemo.Common");
builder.Services.AddSingleton<MigrationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var migrationService = app.Services.GetRequiredService<MigrationService>();
await migrationService.Migrate<Provider>();
await migrationService.Migrate<Area>();
await migrationService.Migrate<Package>();
await migrationService.Migrate<Resource>();
await migrationService.Migrate<PackageResource>();


app.MapDbAccessEndpoints();

app.UseHttpsRedirection();

app.Run();
