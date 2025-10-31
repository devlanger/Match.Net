using MediatR;

namespace MatchmakingService.Application.Queries.Common;

public class PaginatedRequest<T> : IRequest<T>
{
    public int Page { get; set; } = 0;
    public int Size { get; set; } = 10;
}