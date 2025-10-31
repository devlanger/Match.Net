using MatchmakingService.Core.Data;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/matchmaking")]
public class MatchmakingController : ControllerBase
{
    private string heartbeat_url = "http://host.docker.internal:5233/api/matchmaking/heartbeat";

    private List<ServerInstance> Servers => ServerCleanupService.Servers;
    
    [HttpPost("join")]
    public async Task<IActionResult> Join()
    {
        //var server = Servers.FirstOrDefault(s => s.PlayersCount != s.MaxPlayers);
       // if (server == null)
        //{
            // No free server, launch a new one
            ServerInstance server;
            int port = 7777;
            for (int i = 7777; i < 8000; i++)
            {
                if (Servers.Any(s => s.Port == i))
                    continue;

                port = i;
                break;
            }
            
            string containerName = $"unity_server_{port}";
            server = new ServerInstance { ContainerName = containerName, Port = port, PlayersCount = 1, LastHeartbeat = DateTime.UtcNow };
            await ServerCleanupService.LaunchUnityServerAsync(server, containerName, port, heartbeat_url);
            Servers.Add(server);
        //}

        return Ok(new
        {
            ContainerName = server.ContainerName,
            PlayerCount = server.PlayersCount,
            Port = server.Port
        });
    }
    
    [HttpGet("list")]
    public IActionResult List()
    {
        return Ok(Servers.Where(s => !s.IsClosing).Select(s => new
        {
            ContainerName = s.ContainerName,
            PlayerCount = s.PlayersCount,
            Port = s.Port
        }).ToList());
    }

    [HttpPost("heartbeat")]
    public IActionResult Heartbeat([FromBody] HeartbeatPayload heartbeat)
    {
        //Console.WriteLine($"Received heartbeat: {heartbeat.ContainerName}");

        var server = Servers.FirstOrDefault(s => s.ContainerName == heartbeat.ContainerName);
        if (server != null)
        {
            server.LastHeartbeat = DateTime.UtcNow;
            server.PlayersCount = heartbeat.PlayersCount;
        }
        return Ok();
    }

    public class HeartbeatPayload
    {
        public int PlayersCount { get; set; }
        public string ContainerName { get; set; }
    }
}