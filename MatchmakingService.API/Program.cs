using System.Reflection;
using MatchmakingService.Application.Queries.GetServersListQuery;
using MatchmakingService.Core.Data.Config;
using MatchmakingService.DependencyInjection.Extensions;
using MatchmakingService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetServersListQuery).GetTypeInfo().Assembly));
builder.Services.RegisterMatchmakingService();
builder.Services.AddHostedService<ServerCleanupService>();

builder.Services.Configure<MatchmakingConfiguration>(
    builder.Configuration.GetSection("MatchmakingConfiguration"));

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.MapControllers().WithOpenApi();

app.Run();
