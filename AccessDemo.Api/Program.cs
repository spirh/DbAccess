using AccessDemo.Api;
using AccessDemo.Common.Contracts;
using AccessDemo.Common.Services;
using DbAccess.Contracts;
using DbAccess.Helpers;
using DbAccess.Models;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DbAccessConfig>(builder.Configuration.GetRequiredSection("DbAccessConfig"));
DefinitionStore.RegisterAllDefinitions("AccessDemo.Common");
//builder.Services.AddSingleton<IDbConverter, NewDbConverter>();
builder.Services.AddSingleton<IDbConverter, DbConverter>();
builder.Services.AddSingleton<IDbConnection>(new Npgsql.NpgsqlConnection("Database=hhh;Host=localhost;Username=wigg;Password=jw8s0F4;Include Error Detail=true"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAreaService, AreaService>();

builder.Services.AddDbRepositories("AccessDemo.Common");


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDbAccessEndpoints();

app.UseHttpsRedirection();

app.Run();
