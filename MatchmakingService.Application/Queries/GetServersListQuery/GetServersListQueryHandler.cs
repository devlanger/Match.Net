using MatchmakingService.Abstractions;
using MatchmakingService.Application.Models;
using MediatR;

namespace MatchmakingService.Application.Queries.GetServersListQuery;

public class GetServersListQueryHandler(IServerManager serverManager)
    : IRequestHandler<GetServersListQuery, List<ServerInstanceResponseModel>>
{
    public async Task<List<ServerInstanceResponseModel>> Handle(GetServersListQuery request, CancellationToken cancellationToken)
    {
        var result = serverManager.Servers
            .Where(s => !s.IsClosing)
            .Select(s => new ServerInstanceResponseModel(s.ContainerName, s.PlayersCount, s.Address, s.Port, s.MapName))
            .Take(request.Size).Skip(request.Page)
            .ToList();
        
        return await Task.FromResult(result);
    }
}