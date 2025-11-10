using Docker.DotNet;
using Docker.DotNet.Models;
using MatchmakingService.Abstractions;
using MatchmakingService.Core.Data;
using MatchmakingService.Core.Data.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace MatchmakingService.Infrastructure;

public class ServersManager(IOptions<MatchmakingConfiguration> matchmakingConfiguration, ILogger<ServersManager> logger)
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
    
    private DockerClient CreateDockerClient()
    {
        // Dynamically detect Docker endpoint
        var dockerUri = Environment.GetEnvironmentVariable("DOCKER_SOCKET_URL") 
                        ?? (OperatingSystem.IsWindows()
                            ? "npipe://./pipe/docker_engine"
                            : "unix:///var/run/docker.sock");

        logger.LogInformation("Using Docker endpoint: {DockerUri}", dockerUri);
        return new DockerClientConfiguration(new Uri(dockerUri)).CreateClient();
    }
    
    public async Task<ServerInstance> LaunchUnityServerAsync()
    {
        var hostPort = GetFreePort();
        if (hostPort == -1)
        {
            return null;
        }
        
        logger.LogInformation($"DOCKER URL: {NpipePipeDockerEngine()}");
        
        var containerName = $"{_configuration.ContainerName}-{hostPort}";
        var instance = new ServerInstance { ContainerName = containerName, Port = hostPort, PlayersCount = 1, LastHeartbeat = DateTime.UtcNow };
        using var docker = CreateDockerClient();

        // The environment variables to pass
        var env = new List<string> {
            $"CONTAINER_NAME={containerName}",
            $"HEARTBEAT_URL={_configuration.HeartbeatUrl}",
            $"SERVER_PORT={hostPort}",
        };

        // Create container
        var createParams = new CreateContainerParameters
        {
            Image = _configuration.GameServerContainerName,
            Name = containerName,
            Env = env,
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                { $"{_configuration.ContainerExposedPort}/tcp", default(EmptyStruct) },
                { $"{_configuration.ContainerExposedPort}/udp", default(EmptyStruct) }
            },
            HostConfig = new HostConfig
            {
                NetworkMode = "host",
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
        //instance.Address = await GetPublicIpAddress();

        var started = await docker.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

        if (!started)
        {
            throw new Exception($"Failed to start container {containerName}");
        }

        Servers.Add(instance);
        return instance;
    }
    
    private static async Task<string> GetPublicIpAddress()
    {
        using HttpClient client = new HttpClient();
        try
        {
            // Request API to get public IP
            var response = await client.GetAsync("https://api.ipify.org?format=json");
            response.EnsureSuccessStatusCode();
                
            // Parse the returned JSON response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonObject = JObject.Parse(jsonResponse);
                
            // Extract IP address
            return jsonObject["ip"].ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return null;
        }
    }
    
    public static string NpipePipeDockerEngine()
    {
        var env = Environment.GetEnvironmentVariable("DOCKER_SOCKET_URL");
        if (!string.IsNullOrEmpty(env))
            return env;

        return OperatingSystem.IsWindows()
            ? "npipe://./pipe/docker_engine"
            : "unix:///var/run/docker.sock";
    }
}