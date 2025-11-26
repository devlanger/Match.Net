using System.Net;

namespace MatchmakingService.Core.Data;

public class ServerInstance
{
    public string Id { get; set; }
    public string ContainerName { get; set; }
    public int Port { get; set; }
    public string MapName { get; set; }
    public int PlayersCount { get; set; }
    public int MaxPlayers { get; set; } = 5;
    public DateTime LastHeartbeat { get; set; }
    public int LastPlayersCount { get; set; } = -1;
    public bool IsClosing { get; set; }
    public string Address { get; set; }
}