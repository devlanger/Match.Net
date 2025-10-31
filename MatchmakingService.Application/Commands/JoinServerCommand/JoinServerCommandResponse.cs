namespace MatchmakingService.Application.Commands.JoinServerCommand;

public record JoinServerCommandResponse(string ContainerName, int PlayersCount, int Port);
