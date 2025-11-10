using MatchmakingService.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MatchmakingService.Application.Commands.JoinServerCommand;

public class JoinServerCommandHandler(IServerManager serversManager, ILogger<JoinServerCommandHandler> logger) : IRequestHandler<JoinServerCommand, JoinServerCommandResponse>
{
    public async Task<JoinServerCommandResponse> Handle(JoinServerCommand request, CancellationToken cancellationToken)
    {
        logger.LogError("THIS IS UPDATED CODE");
        var serverInstance = await serversManager.LaunchUnityServerAsync();

        return new JoinServerCommandResponse(serverInstance.ContainerName, serverInstance.PlayersCount, serverInstance.Port);
    }
}