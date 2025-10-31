using MatchmakingService.Core.Data;

namespace MatchmakingService.Abstractions;

public interface IServerManager
{
    List<ServerInstance> Servers { get; }
    Task<ServerInstance> LaunchUnityServerAsync();
}