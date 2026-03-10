using OpenClawApi.Application.Interfaces;
using OpenClawApi.Application.Services;
using OpenClawApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Application services
builder.Services.AddScoped<IUserService, UserService>();

// Infrastructure (DbContext + Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Required for integration test WebApplicationFactory
public partial class Program { }
