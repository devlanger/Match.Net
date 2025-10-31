using MatchmakingService.Abstractions;
using MatchmakingService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MatchmakingService.DependencyInjection.Extensions;

public static class MatchmakingServiceCollectionExtensions
{
    public static void RegisterMatchmakingService(this IServiceCollection builder)
    {
        builder.AddSingleton<IServerManager, ServersManager>();
    }
}