using MatchmakingService.Abstractions;
using MatchmakingService.Application.Commands.JoinServerCommand;
using MatchmakingService.Application.Commands.SendHeartbeatCommand;
using MatchmakingService.Application.Models;
using MatchmakingService.Application.Queries.GetServersListQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MatchmakingService.Controllers;

[ApiController]
[Route("api/matchmaking")]
public class MatchmakingController(IServerManager serversManager, IMediator mediator) : ControllerBase
{
    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinServerCommand command) => Ok(await mediator.Send(command));
    
    [HttpGet("list")]
    public async Task<IActionResult> List() => Ok(await mediator.Send(new GetServersListQuery()));

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequestModel heartbeat) => Ok(await mediator.Send(new SendHeartbeatCommand(heartbeat.ContainerName, heartbeat.PlayersCount)));
}