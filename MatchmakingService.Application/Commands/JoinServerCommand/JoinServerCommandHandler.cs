using MatchmakingService.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MatchmakingService.Application.Commands.JoinServerCommand;

public class JoinServerCommandHandler(IServerManager serversManager, ILogger<JoinServerCommandHandler> logger) : IRequestHandler<JoinServerCommand, JoinServerCommandResponse>
{
    public async Task<JoinServerCommandResponse> Handle(JoinServerCommand request, CancellationToken cancellationToken)
    {
        var serverInstance = await serversManager.LaunchUnityServerAsync(request.MapName, cancellationToken);

        return new JoinServerCommandResponse(serverInstance.ContainerName, serverInstance.PlayersCount, serverInstance.Port, serverInstance.MapName);
    }
}