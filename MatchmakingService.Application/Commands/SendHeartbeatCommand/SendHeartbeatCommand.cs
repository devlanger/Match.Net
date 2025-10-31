using MediatR;

namespace MatchmakingService.Application.Commands.SendHeartbeatCommand;

public record SendHeartbeatCommand(string ContainerName, int PlayersCount) : IRequest<int>;
