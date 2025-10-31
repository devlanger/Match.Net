namespace MatchmakingService.Core.Data.Config;

public class MatchmakingConfiguration
{
    public string ContainerName { get; set; } = "container_instance";
    public required string HeartbeatUrl { get; set; }
    public int StartingPort { get; set; } = 7777;
    public int EndingPort { get; set; } = 8000;
    public int ContainerExposedPort { get; set; } = 7777;
}