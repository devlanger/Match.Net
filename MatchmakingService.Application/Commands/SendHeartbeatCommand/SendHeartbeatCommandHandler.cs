using MatchmakingService.Abstractions;
using MediatR;

namespace MatchmakingService.Application.Commands.SendHeartbeatCommand;

public class SendHeartbeatCommandHandler(IServerManager serversManager) : IRequestHandler<SendHeartbeatCommand, int>
{
    public async Task<int> Handle(SendHeartbeatCommand request, CancellationToken cancellationToken)
    {
        var server = serversManager.Servers.FirstOrDefault(s => s.ContainerName == request.ContainerName);
        if (server != null)
        {
            server.LastHeartbeat = DateTime.UtcNow;
            server.PlayersCount = request.PlayersCount;
        }

        return await Task.FromResult(0);
    }
}