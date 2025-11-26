using MediatR;

namespace MatchmakingService.Application.Commands.JoinServerCommand;

public class JoinServerCommand : IRequest<JoinServerCommandResponse>
{
    public string MapName { get; set; }
}