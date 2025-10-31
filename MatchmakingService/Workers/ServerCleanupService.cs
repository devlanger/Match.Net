using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;
using MatchmakingService.Core.Data;

public class ServerCleanupService : BackgroundService
{
    // Reference your server pool (static for example)
    public static List<ServerInstance> Servers = new List<ServerInstance>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            CleanupIdleServers();
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // run every minute
        }
    }

    public static async Task<string> LaunchUnityServerAsync(ServerInstance instance, string containerName, int hostPort, string heartbeatUrl)
    {
        var docker = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"))
            .CreateClient();

        // The environment variables to pass
        var env = new List<string> {
            $"CONTAINER_NAME={containerName}",
            $"HEARTBEAT_URL={heartbeatUrl}",
            $"SERVER_PORT={hostPort}"  // if your image reads SERVER_PORT
        };

        // Create container
        var createParams = new CreateContainerParameters
        {
            Image = "nin-server",  // your Unity server Docker image name
            Name = containerName,
            Env = env,
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                { "7777/tcp", default(EmptyStruct) },  // internal port that Unity listens on
                { "7777/udp", default(EmptyStruct) }  // internal port that Unity listens on
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        "7777/tcp",
                        new List<PortBinding> {
                            new PortBinding {
                                HostPort = hostPort.ToString()
                            }
                        }
                    },
                    {
                        "7777/udp",
                        new List<PortBinding> {
                            new PortBinding {
                                HostPort = hostPort.ToString()
                            }
                        }
                    }
                }
            }
        };

        var response = await docker.Containers.CreateContainerAsync(createParams);
        string containerId = response.ID;
        instance.Id = containerId;

        // Start container
        bool started = await docker.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

        if (!started)
        {
            throw new Exception($"Failed to start container {containerName}");
        }

        return containerId;
    }
    
    private void CleanupIdleServers()
    {
        Console.WriteLine("Clearing Idle Servers");
        foreach (var server in Servers.ToList())
        {
            if ((DateTime.UtcNow - server.LastHeartbeat).TotalSeconds > 20 || (server.PlayersCount <= 1 && server.LastPlayersCount == server.PlayersCount)) // idle threshold
            {
                server.IsClosing = true;
                var docker = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"))
                    .CreateClient();
                
                var response = docker.Containers.RemoveContainerAsync(server.Id, new ContainerRemoveParameters()
                {
                    Force = true
                });

                Console.WriteLine($"Stopping idle server: {server.ContainerName}");
                Process.Start("docker", $"stop {server.ContainerName}");
                Servers.Remove(server);
            }
            server.LastPlayersCount = server.PlayersCount;
        }
    }
}