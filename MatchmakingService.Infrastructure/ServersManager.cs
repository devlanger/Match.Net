using Docker.DotNet;
using Docker.DotNet.Models;
using MatchmakingService.Abstractions;
using MatchmakingService.Core.Data;
using MatchmakingService.Core.Data.Config;
using Microsoft.Extensions.Options;

namespace MatchmakingService.Infrastructure;

public class ServersManager(IOptions<MatchmakingConfiguration> matchmakingConfiguration)
    : IServerManager
{
    private readonly MatchmakingConfiguration _configuration = matchmakingConfiguration.Value;

    public List<ServerInstance> Servers { get; } = [];

    private int GetFreePort()
    {
        for (var i = _configuration.StartingPort; i < _configuration.EndingPort; i++)
        {
            if (Servers.Any(s => s.Port == i))
                continue;

            return i;
        }

        return -1;
    }
    
    public async Task<ServerInstance> LaunchUnityServerAsync()
    {
        var hostPort = GetFreePort();
        if (hostPort == -1)
        {
            return null;
        }
        
        var containerName = $"{_configuration.ContainerName}-{hostPort}";
        var instance = new ServerInstance { ContainerName = containerName, Port = hostPort, PlayersCount = 1, LastHeartbeat = DateTime.UtcNow };

        var docker = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"))
            .CreateClient();

        // The environment variables to pass
        var env = new List<string> {
            $"CONTAINER_NAME={containerName}",
            $"HEARTBEAT_URL={_configuration.HeartbeatUrl}",
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
                { $"{_configuration.ContainerExposedPort}/tcp", default(EmptyStruct) },  // internal port that Unity listens on
                { $"{_configuration.ContainerExposedPort}/udp", default(EmptyStruct) }  // internal port that Unity listens on
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        $"{_configuration.ContainerExposedPort}/tcp",
                        new List<PortBinding> {
                            new PortBinding {
                                HostPort = hostPort.ToString()
                            }
                        }
                    },
                    {
                        $"{_configuration.ContainerExposedPort}/udp",
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
        var containerId = response.ID;
        instance.Id = containerId;

        // Start container
        var started = await docker.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

        if (!started)
        {
            throw new Exception($"Failed to start container {containerName}");
        }

        Servers.Add(instance);
        return instance;
    }
}