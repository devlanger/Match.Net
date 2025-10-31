using System.Diagnostics;
using MatchmakingService.Abstraction;
using Docker.DotNet;
using Docker.DotNet.Models;
using MatchmakingService.Abstractions;
using MatchmakingService.Core.Data;
using Microsoft.Extensions.Hosting;

namespace MatchmakingService.Infrastructure;

public class ServerCleanupService(IServerManager serversManager) : BackgroundService, IServerCleanupService
{
    private static string DockerUrl()
    {
        return "npipe://./pipe/docker_engine";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            CleanupIdleServers();
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private void CleanupIdleServers()
    {
        Console.WriteLine("Clearing Idle Servers");
        foreach (var server in serversManager.Servers.ToList())
        {
            if (IsServerReadyForCleanup(server))
            {
                server.IsClosing = true;
                var docker = new DockerClientConfiguration(new Uri(DockerUrl()))
                    .CreateClient();
                
                var response = docker.Containers.RemoveContainerAsync(server.Id, new ContainerRemoveParameters()
                {
                    Force = true
                });

                Console.WriteLine($"Stopping idle server: {server.ContainerName}");
                Process.Start("docker", $"stop {server.ContainerName}");
                serversManager.Servers.Remove(server);
            }
            server.LastPlayersCount = server.PlayersCount;
        }
    }
    
    private bool IsServerReadyForCleanup(ServerInstance server) =>
        (DateTime.UtcNow - server.LastHeartbeat).TotalSeconds > 20 || (server.PlayersCount <= 0 && server.LastPlayersCount == server.PlayersCount);
}