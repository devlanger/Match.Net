using MatchmakingService.Application.Models;
using MatchmakingService.Application.Queries.Common;

namespace MatchmakingService.Application.Queries.GetServersListQuery;

public class GetServersListQuery : PaginatedRequest<List<ServerInstanceResponseModel>>
{
}