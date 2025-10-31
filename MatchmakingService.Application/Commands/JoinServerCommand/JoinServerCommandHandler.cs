using MatchmakingService.Abstractions;
using MediatR;

namespace MatchmakingService.Application.Commands.JoinServerCommand;

public class JoinServerCommandHandler(IServerManager serversManager) : IRequestHandler<JoinServerCommand, JoinServerCommandResponse>
{
    public async Task<JoinServerCommandResponse> Handle(JoinServerCommand request, CancellationToken cancellationToken)
    {
        var serverInstance = await serversManager.LaunchUnityServerAsync();

        return new JoinServerCommandResponse(serverInstance.ContainerName, serverInstance.PlayersCount, serverInstance.Port);
    }
}