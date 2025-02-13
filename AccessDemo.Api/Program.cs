using AccessDemo.Api;
using DbAccess.Contracts;
using DbAccess.Helpers;
using DbAccess.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DbAccessConfig>(builder.Configuration.GetRequiredSection("DbAccessConfig"));
DefinitionStore.RegisterAllDefinitions("AccessDemo.Common");

builder.Services.AddSingleton<IDbConverter, DbConverter>();
//builder.Services.AddSingleton<IDbConverter, OldDbConverter>();

var dataSource = NpgsqlDataSource.Create("Database=hhh;Host=localhost;Username=wigg;Password=jw8s0F4;Include Error Detail=true");
builder.Services.AddSingleton<NpgsqlDataSource>(dataSource);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbServices("AccessDemo.Common");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDbAccessEndpoints();

app.UseHttpsRedirection();

app.Run();
