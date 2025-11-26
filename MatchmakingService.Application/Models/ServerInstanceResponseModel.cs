namespace MatchmakingService.Application.Models;

public record ServerInstanceResponseModel(string ContainerName, int PlayerCount, string Address, int Port, string MapName);
