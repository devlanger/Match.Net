namespace MatchmakingService.Application.Models;

public record ServerInstanceResponseModel(string ContainerName, int PlayerCount, int Port);
