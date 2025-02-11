using DbAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbAccess(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.UseDbAccess();
}

app.UseHttpsRedirection();

app.Run();
